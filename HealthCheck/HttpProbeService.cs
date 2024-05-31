using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
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
    ///     2.  HTTP 503 Service Unavailable if just ONE Health Check Service returns a Degraded or Unhealthy result
    /// f. Other types of responses are as follows
    ///     1.  If the request is not a HTTP GET method, then a 405 Method Not Allowed is returned
    ///     2.  If an endpoint is not matched, then a 404 Not Found is returned
    /// </remarks>
    internal class HttpProbeService : IHttpProbeService
    {
        private const int MAX_REQUEST_MESSAGE_SIZE = 1024;
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
            // Yield so that caller can continue processing
            await Task.Yield();

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
                                string requestMessage = await GetRequestMessage(client);

                                // Ensure that request message starts with HTTP GET Method/Verb
                                if (!requestMessage.StartsWith(HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase))
                                {
                                    var notAllowedResponse = GenerateHttpResponse(HttpStatusCode.MethodNotAllowed);
                                    await SendResponse(client, notAllowedResponse, cancellationToken);
                                    continue;       // Bypass all other code
                                }

                                // Parse endpoint from request message and determine if it is one of our health check endpoints
                                string endpoint = GetRequestedEndpoint(requestMessage);                         
                                HealthCheckType healthCheckType = GetHealthCheckType(endpoint, healthCheckEndpoints);

                                // Send appropriate response based upon Health Check Type
                                if (healthCheckType == HealthCheckType.Unknown)
                                {
                                    var notFoundResponse = GenerateHttpResponse(HttpStatusCode.NotFound);
                                    await SendResponse(client, notFoundResponse, cancellationToken);
                                    continue;       // Bypass all other code
                                }


                                // Process valid health check
                                LogProbe(loggingOptions, healthCheckType);
                                HealthCheckResults healthCheckResults = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
                                var okResponse = CreateResponse(healthCheckType, healthCheckResults);
                                await SendResponse(client, okResponse, cancellationToken);
                                LogHealthCheck(loggingOptions, healthCheckResults);
                            }

                            catch(Exception ex)
                            {
                                _logger.LogError(ex, "Http Probe Service failed when processing request");
                            }
                        }
                    }
                }
            }

            catch (OperationCanceledException)
            { }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Http Probe Service encountered an error and is shutting down");
            }


            await Task.CompletedTask;
        }


        #region HTTP Request Processes
        private async Task<string> GetRequestMessage(TcpClient client)
        {
            var buffer = new byte[MAX_REQUEST_MESSAGE_SIZE];
            var stream = client.GetStream();
            var length = await stream.ReadAsync(buffer, 0, MAX_REQUEST_MESSAGE_SIZE);
            return Encoding.UTF8.GetString(buffer, 0, length).Trim();
        }

        /// <summary>
        /// Parse the request message to get the requested endpoint
        /// </summary>
        /// <param name="requestMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// This needs to be as fast as possible, therefore the use of Spans and Char arrays
        /// </remarks>
        private string GetRequestedEndpoint(ReadOnlySpan<char> requestMsg)
        {
            const char CARRIAGE_RETURN = '\r';
            const char LINE_FEED = '\n';
            const char SPACE = ' ';
            const char PARAMETER_TOKEN = '?';

            // Read Past Http Method and get endpoint
            int methodLength = HttpMethod.Get.Method.Length;
            int messageLength = requestMsg.Length;
            char[] endpoint = new char[MAX_REQUEST_MESSAGE_SIZE];
            int cnt = 0;

            for(int i = methodLength; i < messageLength && cnt < MAX_REQUEST_MESSAGE_SIZE; i++)
            {
                char c = requestMsg[i];

                // Skip leading spaces (We know leading spaces because of the value of cnt)
                if (c == SPACE && cnt == 0)
                    continue;

                // Break out if we encounter any of the following
                if (c == SPACE ||                       // Trailing space means we have reached the end of the requested endpoint
                    c == PARAMETER_TOKEN ||             // Start of Parameters, which are not needed
                    c == CARRIAGE_RETURN ||             // Extra precaution
                    c == LINE_FEED)                     // Extra precaution
                    break;

                // Build endpoint
                endpoint[cnt++] = c;
            }

            return AppendTrailingForwardSlash(new string(endpoint, 0, cnt));
        }


        private string AppendTrailingForwardSlash(ReadOnlySpan<char> endpoint)
        {
            const char FORWARD_SLASH = '/';
            var length = endpoint.Length;
            return (length > 0 && endpoint[length - 1] == FORWARD_SLASH)
                        ? new string(endpoint)
                        : String.Concat(new string(endpoint), FORWARD_SLASH);
        }
        #endregion


        #region HTTP Response Processes
        private IDictionary<HealthCheckType, string> CreateHealthCheckEndpointDictionary(EndpointAssignment endpointAssignment)
        {
            IDictionary<HealthCheckType, string> ret = new Dictionary<HealthCheckType, string>();

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Status))
                ret.Add(HealthCheckType.Status, AppendTrailingForwardSlash(endpointAssignment.Status));

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Startup))
                ret.Add(HealthCheckType.Startup, AppendTrailingForwardSlash(endpointAssignment.Startup));

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Readiness))
                ret.Add(HealthCheckType.Readiness, AppendTrailingForwardSlash(endpointAssignment.Readiness));

            if (!String.IsNullOrWhiteSpace(endpointAssignment.Liveness))
                ret.Add(HealthCheckType.Liveness, AppendTrailingForwardSlash(endpointAssignment.Liveness));

            return ret;
        }

        private HealthCheckType GetHealthCheckType(string endpoint, IDictionary<HealthCheckType, string> expectedEndpoints)
        {
            return expectedEndpoints.Where(kvp => endpoint.EndsWith(kvp.Value, StringComparison.OrdinalIgnoreCase))
                                    .Select(kvp => kvp.Key)
                                    .FirstOrDefault();
        }


        private string CreateResponse(HealthCheckType healthCheckType, HealthCheckResults healthCheckResults)
        {
            string httpResponse;
            switch (healthCheckType)
            {
                case HealthCheckType.Status:
                    httpResponse = GenerateHttpResponse(HttpStatusCode.OK, healthCheckResults);
                    break;

                default:
                    if (healthCheckResults.HealthStatus == HealthStatus.Healthy)
                        httpResponse = GenerateHttpResponse(HttpStatusCode.OK);
                    else
                        httpResponse = GenerateHttpResponse(HttpStatusCode.ServiceUnavailable);
                    break;
            }

            return httpResponse;
        }

        private string GenerateHttpResponse(HttpStatusCode httpStatusCode, HealthCheckResults? healthCheckResults = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("HTTP/1.1 {0} {1}", (int)httpStatusCode, httpStatusCode.ToString());
            sb.AppendLine();
            
            if (healthCheckResults != null)
            {
                string json = Json.Serialize(healthCheckResults);
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


            return sb.ToString();
        }


        private async Task SendResponse(TcpClient client, string httpResponse, CancellationToken cancellationToken)
        {
            byte[] http = Encoding.UTF8.GetBytes(httpResponse);
            var stream = client.GetStream();
            await stream.WriteAsync(http, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        #endregion


        #region Logging
        private void LogProbe(ProbeLoggingOptions loggingOptions, HealthCheckType healthCheckType)
        {
            if (loggingOptions.LogProbe)
                _logger.LogInformation("Health Check Probe: {0}", healthCheckType.ToString());
        }


        private void LogHealthCheck(ProbeLoggingOptions loggingOptions, HealthCheckResults healthCheckResults)
        {
            if (loggingOptions.LogStatusWhenHealthy &&
                healthCheckResults.HealthStatus == HealthStatus.Healthy)
            {
                _logger.LogInformation("Health Check Result: {0}", healthCheckResults.OverallStatus);
            }

            if (loggingOptions.LogStatusWhenNotHealthy &&
                healthCheckResults.HealthStatus != HealthStatus.Healthy)
            {
                _logger.LogWarning("Health Check Result: {0}", healthCheckResults.OverallStatus);
                _logger.LogWarning("Health Check Detailed Results: {0}", Json.Serialize(healthCheckResults));
            }
        }
        #endregion
    }
}
