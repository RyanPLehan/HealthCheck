using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using HealthCheck.Configuration;
using System.Text;

namespace HealthCheck
{
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
            Task task = Task.CompletedTask;

            try
            {
                using (TcpListener listener = new TcpListener(IPAddress.Any, probeOptions.Port))
                {
                    listener.Start();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                        var endpoint = await GetRequestedEndpoint(client);
                        endpoint = StripParameters(endpoint);
                        endpoint = NormalizeEndPoint(endpoint);
                        var healthCheckType = GetHealthCheckType(endpoint, probeOptions.Endpoints.ToDictionary());

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


        private async Task<string> GetRequestedEndpoint(TcpClient client)
        {
            const int BUFFER_SZIE = 10240;
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

            return endpoint;
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

        private HealthCheckType GetHealthCheckType(string endpoint, IDictionary<HealthCheckType, string> expectedEndpoints)
        {
            return expectedEndpoints.Where(kvp => endpoint.EndsWith(NormalizeEndPoint(kvp.Value), StringComparison.OrdinalIgnoreCase))
                                    .Select(kvp => kvp.Key)
                                    .FirstOrDefault();
        }
    }
}
