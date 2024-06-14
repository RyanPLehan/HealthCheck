using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using HealthCheck.Configuration;
using HealthCheck.Formatters;

namespace HealthCheck.Services
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
    internal abstract class HttpService
    {
        protected const int MAX_REQUEST_MESSAGE_SIZE = 1024;
        private readonly ILogger _logger;
        private readonly IHealthCheckService _healthCheckService;


        protected HttpService(ILogger logger,
                              IHealthCheckService healthCheckService)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));

            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        protected ILogger Logger { get => _logger; }

        protected async Task Execute(EndpointAssignment endpoints, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            IDictionary<HealthCheckType, string> healthCheckEndpoints = CreateHealthCheckEndpointDictionary(endpoints);

            // Double check to make sure there is at least one endpoint
            if (!healthCheckEndpoints.Any())
            {
                _logger.LogWarning("No endpoints defined.  HTTP Monitor shutting down.");
                await Task.CompletedTask;
                return;
            }


            using (TcpListener listener = CreateTcpListener())
            {
                listener.Start();
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken))
                    {
                        try
                        {
                            Stream clientStream = await GetClientStream(client);
                            string requestMessage = await GetRequestMessage(clientStream);

                            // Ensure that request message starts with HTTP GET Method/Verb
                            if (!requestMessage.StartsWith(HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase))
                            {
                                var notAllowedResponse = GenerateHttpResponse(HttpStatusCode.MethodNotAllowed);
                                await SendResponseMessage(clientStream, notAllowedResponse, cancellationToken);
                                continue;       // Bypass all other code
                            }

                            // Parse endpoint from request message and determine if it is one of our health check endpoints
                            string endpoint = GetRequestedEndpoint(requestMessage);
                            HealthCheckType healthCheckType = GetHealthCheckType(endpoint, healthCheckEndpoints);

                            // Send appropriate response based upon Health Check Type
                            if (healthCheckType == HealthCheckType.Unknown)
                            {
                                var notFoundResponse = GenerateHttpResponse(HttpStatusCode.NotFound);
                                await SendResponseMessage(clientStream, notFoundResponse, cancellationToken);
                                continue;       // Bypass all other code
                            }


                            // Process valid health check
                            LoggingService.LogProbe(_logger, loggingOptions, healthCheckType);
                            HealthReport healthReport = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
                            var okResponse = CreateResponse(healthCheckType, healthReport);
                            await SendResponseMessage(clientStream, okResponse, cancellationToken);
                            LoggingService.LogHealthCheck(_logger, loggingOptions, healthReport);
                        }

                        catch (AuthenticationException ex)
                        {
                            _logger.LogError(ex, "Authentication failed - closing connection");
                        }

                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Http Service failed when processing request");
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }


        #region Abstract and default protected methods
        protected abstract TcpListener CreateTcpListener();

        protected virtual Task<Stream> GetClientStream(TcpClient client) => Task.FromResult<Stream>(client.GetStream());
        #endregion


        #region HTTP Request Processes
        /// <summary>
        /// Get HTTP Request Message
        /// </summary>
        /// <param name="clientStream"></param>
        /// <returns></returns>
        private async Task<string> GetRequestMessage(Stream clientStream)
        {
            var buffer = new byte[MAX_REQUEST_MESSAGE_SIZE];
            var length = await clientStream.ReadAsync(buffer, 0, MAX_REQUEST_MESSAGE_SIZE);
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

            for (int i = methodLength; i < messageLength && cnt < MAX_REQUEST_MESSAGE_SIZE; i++)
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
            return length > 0 && endpoint[length - 1] == FORWARD_SLASH
                        ? new string(endpoint)
                        : string.Concat(new string(endpoint), FORWARD_SLASH);
        }
        #endregion


        #region HTTP Response Processes
        /// <summary>
        /// Send HTTP Response Message back to client
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="httpResponse"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendResponseMessage(Stream clientStream, string httpResponse, CancellationToken cancellationToken)
        {
            byte[] http = Encoding.UTF8.GetBytes(httpResponse);
            await clientStream.WriteAsync(http, cancellationToken);
            await clientStream.FlushAsync(cancellationToken);
        }


        private string CreateResponse(HealthCheckType healthCheckType, HealthReport healthReport)
        {
            string httpResponse;
            switch (healthCheckType)
            {
                case HealthCheckType.Status:
                    httpResponse = GenerateHttpResponse(HttpStatusCode.OK, healthReport);
                    break;

                default:
                    if (healthReport.Status == HealthStatus.Healthy)
                        httpResponse = GenerateHttpResponse(HttpStatusCode.OK);
                    else
                        httpResponse = GenerateHttpResponse(HttpStatusCode.ServiceUnavailable);
                    break;
            }

            return httpResponse;
        }

        private string GenerateHttpResponse(HttpStatusCode httpStatusCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("HTTP/1.1 {0} {1}", (int)httpStatusCode, httpStatusCode.ToString());
            sb.AppendLine();
            sb.Append("Content-Length: 0");
            sb.AppendLine();
            sb.AppendLine();        // Must provide 2 \r\n

            return sb.ToString();
        }

        private string GenerateHttpResponse(HttpStatusCode httpStatusCode, HealthReport healthReport)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("HTTP/1.1 {0} {1}", (int)httpStatusCode, httpStatusCode.ToString());
            sb.AppendLine();

            string json = CreateHealthCheckResponse(healthReport);
            sb.AppendFormat("Content-Length: {0}", Encoding.UTF8.GetByteCount(json));
            sb.AppendLine();
            sb.AppendFormat("Content-Type: {0}", Json.JSON_CONTENT_TYPE);
            sb.AppendLine();
            sb.AppendLine();        // Must provide 2 \r\n before content
            sb.Append(json);
            sb.AppendLine();

            return sb.ToString();
        }

        private string CreateHealthCheckResponse(HealthReport report)
        {
            return Json.Serialize(
                new
                {
                    Status = report.Status.ToString(),
                    HealthChecks = report.Entries.Select(e => new
                    {
                        Key = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Data = e.Value.Data,
                        Exception = e.Value.Exception?.Message,
                    }),
                }
            );
        }
        #endregion


        private IDictionary<HealthCheckType, string> CreateHealthCheckEndpointDictionary(EndpointAssignment endpointAssignment)
        {
            IDictionary<HealthCheckType, string> ret = new Dictionary<HealthCheckType, string>();

            if (!string.IsNullOrWhiteSpace(endpointAssignment?.Status))
                ret.Add(HealthCheckType.Status, AppendTrailingForwardSlash(endpointAssignment.Status));

            if (!string.IsNullOrWhiteSpace(endpointAssignment?.Startup))
                ret.Add(HealthCheckType.Startup, AppendTrailingForwardSlash(endpointAssignment.Startup));

            if (!string.IsNullOrWhiteSpace(endpointAssignment?.Readiness))
                ret.Add(HealthCheckType.Readiness, AppendTrailingForwardSlash(endpointAssignment.Readiness));

            if (!string.IsNullOrWhiteSpace(endpointAssignment?.Liveness))
                ret.Add(HealthCheckType.Liveness, AppendTrailingForwardSlash(endpointAssignment.Liveness));

            return ret;
        }

        private HealthCheckType GetHealthCheckType(string endpoint, IDictionary<HealthCheckType, string> expectedEndpoints)
        {
            return expectedEndpoints.Where(kvp => endpoint.EndsWith(kvp.Value, StringComparison.OrdinalIgnoreCase))
                                    .Select(kvp => kvp.Key)
                                    .FirstOrDefault();
        }
    }
}
