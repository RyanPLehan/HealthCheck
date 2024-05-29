using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json.Serialization;
using HealthCheck.Configuration;
using HealthCheck.Formatters;

namespace HealthCheck
{
    /// <summary>
    /// This will respond to HTTP probes using the given port by issuing HTTP 200 or HTTP 503 status codes
    /// </summary>
    /// <remarks>
    /// This is a bare minimum HTTP Server that will specifically look for endpoints.
    /// If a matching endpoint is found, then it will execute the Health Check services for the associated health check type
    /// This is a typical Request/Response in that
    /// a. A HTTP GET request is made to a specific end point
    /// b. The endpoint is scanned to determine if it is a Health Check endpoint
    /// c. Execution of one or more Health Checks for that endpoint is executed and compiled into a single object
    /// d. If the endpoint is registered to Health Check Status Request, then the response will send a Json object of all Health Check Service Results
    /// e. If the endpoint is registered to Health Check Startup, Readiness or Liveness then one of the following responses will be returned
    ///     1.  HTTP 200 OK if ALL Health Check Services returns Healthy
    ///     2.  HTTP 302 Service Unavailable if just ONE Health Check Service returns a Degraded or Unhealthy result
    /// </remarks>
    internal class HttpProbeService : IHttpProbeService
    {
        private readonly ILogger<HttpProbeService> _logger;
        private readonly IHealthCheckService _healthCheckService;


        public HttpProbeService(ILogger<HttpProbeService> logger,
                                IHealthCheckService healthCheckService)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));

            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        public async Task Monitor(HttpProbeOptions probeOptions, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            IDictionary<HealthCheckType, string> healthCheckEndpoints = CreateHealthCheckEndpointDictionary(probeOptions.Endpoints);

            try
            {
                using (TcpListener listener = new TcpListener(IPAddress.Any, probeOptions.Port))
                {
                    listener.Start();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        using (TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken))
                        {
                            try
                            {
                                string endpoint = await GetRequestedEndpoint(client);
                                HealthCheckType healthCheckType = GetHealthCheckType(endpoint, healthCheckEndpoints);
                                if (healthCheckType != HealthCheckType.Unknown)
                                {
                                    LogProbe(loggingOptions, healthCheckType);
                                    HealthCheckOverallStatus healthCheckOverallStatus = await ProcessHealthCheck(client, healthCheckType, cancellationToken);
                                    LogHealthCheck(loggingOptions, healthCheckOverallStatus);
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex, "Http Probe Service failed when processing request");
                            }
                        }
                    }
                }
            }

            catch (OperationCanceledException ex)
            { }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Http Probe Service encountered an error and is shutting down");
            }


            await Task.CompletedTask;
        }


        #region HTTP Request Processes
        private async Task<string> GetRequestedEndpoint(TcpClient client)
        {
            const int BUFFER_SZIE = 256;
            const string HTTP_GET = "GET";
            const char SPACE = ' ';
            const string DEFAULT_ENDPOINT = "/";

            string endpoint = DEFAULT_ENDPOINT;

            var buffer = new byte[BUFFER_SZIE];
            var stream = client.GetStream();
            var length = await stream.ReadAsync(buffer, 0, BUFFER_SZIE);
            var requestMsg = Encoding.UTF8.GetString(buffer, 0, length);
            var requestVerb = FindRequestVerb(requestMsg, HTTP_GET);

            if (!String.IsNullOrWhiteSpace(requestVerb))
            {
                var parsedVerb = requestVerb.Split(SPACE);
                if (parsedVerb.Length >= 2)
                    endpoint = parsedVerb[1];
            }

            return NormalizeEndPoint(StripParameters(endpoint));
        }

        private string FindRequestVerb(string requestMsg, string verb)
        {
            const string CRLF = "\r\n";
            string ret = null;
            var parsedMsg = requestMsg.Split(CRLF);

            foreach (string item in parsedMsg)
            {
                if (item.StartsWith(verb, StringComparison.OrdinalIgnoreCase))
                {
                    ret = item;
                    break;
                }
            }

            return ret;
        }

        private string StripParameters(ReadOnlySpan<char> endpoint)
        {
            const char PARAMETER_TOKEN = '?';

            int length = endpoint.Length;
            int foundIndex = length;
            for (int i = 0; i < length; i++)
            {
                if (endpoint[i] == PARAMETER_TOKEN)
                {
                    foundIndex = i;
                    break;
                }
            }

            return new string(endpoint.Slice(0, foundIndex));
        }

        private string NormalizeEndPoint(ReadOnlySpan<char> endpoint)
        {
            const char FORWARD_SLASH = '/';
            string ret = null;

            if (endpoint[endpoint.Length - 1] == FORWARD_SLASH)
                ret = new string(endpoint);
            else
                ret = String.Concat(new string(endpoint), FORWARD_SLASH);

            return ret;
        }
        #endregion


        #region HTTP Response Processes
        private IDictionary<HealthCheckType, string> CreateHealthCheckEndpointDictionary(EndpointAssignment endpointAssignment)
        {
            IDictionary<HealthCheckType, string> ret = new Dictionary<HealthCheckType, string>();

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Status))
                ret.Add(HealthCheckType.Status, endpointAssignment.Status);

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Startup))
                ret.Add(HealthCheckType.Startup, endpointAssignment.Startup);

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Readiness))
                ret.Add(HealthCheckType.Readiness, endpointAssignment.Readiness);

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Liveness))
                ret.Add(HealthCheckType.Liveness, endpointAssignment.Liveness);

            return ret;
        }

        private HealthCheckType GetHealthCheckType(string endpoint, IDictionary<HealthCheckType, string> expectedEndpoints)
        {
            return expectedEndpoints.Where(kvp => endpoint.EndsWith(NormalizeEndPoint(kvp.Value), StringComparison.OrdinalIgnoreCase))
                                    .Select(kvp => kvp.Key)
                                    .FirstOrDefault();
        }

        private async Task<HealthCheckOverallStatus> ProcessHealthCheck(TcpClient client, HealthCheckType healthCheckType, CancellationToken cancellationToken)
        {
            HealthCheckOverallStatus healthCheckOverallStatus = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
            await SendResponse(client, healthCheckType, healthCheckOverallStatus, cancellationToken);
            return healthCheckOverallStatus;
        }

        private async Task SendResponse(TcpClient client, HealthCheckType healthCheckType, HealthCheckOverallStatus healthCheckOverallStatus, CancellationToken cancellationToken)
        {
            byte[] httpResponse;
            switch (healthCheckType)
            {
                case HealthCheckType.Status:
                    httpResponse = GenerateHttpReponse(HttpStatusCode.OK, healthCheckOverallStatus);
                    break;

                default:
                    if (healthCheckOverallStatus.HealthStatus == HealthStatus.Healthy)
                        httpResponse = GenerateHttpReponse(HttpStatusCode.OK, null);
                    else
                        httpResponse = GenerateHttpReponse(HttpStatusCode.ServiceUnavailable, null);
                    break;
            }

            var stream = client.GetStream();
            await stream.WriteAsync(httpResponse, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private byte[] GenerateHttpReponse(HttpStatusCode httpStatusCode, HealthCheckOverallStatus? healthCheckOverallStatus)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("HTTP/1.1 {0} {1}", (int)httpStatusCode, httpStatusCode.ToString());
            sb.AppendLine();
            
            if (healthCheckOverallStatus != null)
            {
                string json = Json.Serialize(healthCheckOverallStatus);
                sb.AppendFormat("Content-Length: {0}", Encoding.UTF8.GetByteCount(json));
                sb.AppendLine();
                sb.AppendFormat("Content-Type: {0}", Json.JSON_CONTENT_TYPE);
                sb.AppendLine();
                sb.AppendLine();        // Must provide 2 \r\n before content
                sb.Append(json);
                sb.AppendLine();
            }
            else
            {
                sb.Append("Content-Length: 0");
                sb.AppendLine();
                sb.AppendLine();        // Must provide 2 \r\n
            }


            return Encoding.UTF8.GetBytes(sb.ToString());
        }
        #endregion


        #region Logging
        private void LogProbe(ProbeLoggingOptions loggingOptions, HealthCheckType healthCheckType)
        {
            if (loggingOptions.LogProbe)
                _logger.LogInformation("Health Check Probe: {0}", healthCheckType.ToString());
        }


        private void LogHealthCheck(ProbeLoggingOptions loggingOptions, HealthCheckOverallStatus healthCheckOverallStatus)
        {
            if (loggingOptions.LogStatusWhenHealthy &&
                healthCheckOverallStatus.HealthStatus == HealthStatus.Healthy)
            {
                _logger.LogInformation("Health Check Result: {0}", healthCheckOverallStatus.OverallStatus);
            }

            if (loggingOptions.LogStatusWhenNotHealthy &&
                healthCheckOverallStatus.HealthStatus != HealthStatus.Healthy)
            {
                _logger.LogWarning("Health Check Result: {0}", healthCheckOverallStatus.OverallStatus);
                _logger.LogWarning("Health Check Detailed Results: {0}", Json.Serialize(healthCheckOverallStatus));
            }
        }
        #endregion
    }
}
