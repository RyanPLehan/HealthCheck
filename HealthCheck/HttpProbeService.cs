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
    internal class HttpProbeService : IHttpProbeService
    {
        private readonly ILogger<HttpProbeService> _logger;
        private readonly IHealthCheckService _healthCheckService;

        private class HealthCheckOverallStatus
        {
            private readonly HealthStatus _healthStatus;
            private readonly IEnumerable<KeyValuePair<string, HealthCheckResult>> _results;

            public HealthCheckOverallStatus(HealthStatus healthStatus, IEnumerable<KeyValuePair<string, HealthCheckResult>> results)
            {
                _healthStatus = healthStatus;
                _results = results;
            }

            [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
            public HealthStatus HealthStatus 
                { get => _healthStatus; }
            
            public string OverallStatus 
                { get => _healthStatus.ToString(); }

            public IEnumerable<KeyValuePair<string, string>> HealthChecks 
                { get => _results.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Status.ToString())); }
        }

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
            Task task = Task.CompletedTask;
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
                                    HealthCheckOverallStatus healthCheckOverallStatus = await ProcessHealthCheck(client, healthCheckType, cancellationToken);
                                    LogHealthCheck(loggingOptions, healthCheckType, healthCheckOverallStatus);
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex, "Health Check failed when processing request");
                            }
                        }
                    }
                }
            }

            catch (OperationCanceledException ex)
            {
                task = Task.CompletedTask;
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Http Probe Service encountered an error and is shutting down");
                task = Task.FromException(ex);
            }
        }


        #region HTTP Request Processes
        private async Task<string> GetRequestedEndpoint(TcpClient client)
        {
            const int BUFFER_SZIE = 500;
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

        #region Health Check Processes
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
            IEnumerable<KeyValuePair<string, HealthCheckResult>> results = await ExecuteHealthChecks(healthCheckType, cancellationToken);
            HealthStatus overallHealthStatus = DetermineOverallStatus(results);
            HealthCheckOverallStatus healthCheckOverallStatus = new HealthCheckOverallStatus(overallHealthStatus, results);
            await SendResponse(client, healthCheckType, healthCheckOverallStatus, cancellationToken);

            return healthCheckOverallStatus;
        }


        private async Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> ExecuteHealthChecks(HealthCheckType healthCheckType, CancellationToken cancellationToken)
        {
            IEnumerable<KeyValuePair<string, HealthCheckResult>> results = Enumerable.Empty<KeyValuePair<string, HealthCheckResult>>();

            switch (healthCheckType)
            {
                case HealthCheckType.Status:
                    results = await _healthCheckService.CheckStatus(cancellationToken);
                    break;

                case HealthCheckType.Startup:
                    results = await _healthCheckService.CheckStartup(cancellationToken);
                    break;

                case HealthCheckType.Readiness:
                    results = await _healthCheckService.CheckReadiness(cancellationToken);
                    break;

                case HealthCheckType.Liveness:
                    results = await _healthCheckService.CheckLiveness(cancellationToken);
                    break;
            }

            return results;
        }

        private HealthStatus DetermineOverallStatus(IEnumerable<KeyValuePair<string, HealthCheckResult>> results)
        {
            HealthStatus ret = HealthStatus.Healthy;

            if (results.Any(kvp => kvp.Value.Status == HealthStatus.UnHealthy))
                ret = HealthStatus.UnHealthy;
            else if (results.Any(kvp => kvp.Value.Status == HealthStatus.Degraded))
                ret = HealthStatus.Degraded;

            return ret;
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
                sb.AppendLine();
            }


            return Encoding.UTF8.GetBytes(sb.ToString());
        }
        #endregion

        private void LogHealthCheck(ProbeLoggingOptions loggingOptions, HealthCheckType healthCheckType, HealthCheckOverallStatus healthCheckOverallStatus)
        {
            if (loggingOptions.LogProbe)
                _logger.LogInformation("Health Check Probe: {0}", healthCheckType.ToString());

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
    }
}
