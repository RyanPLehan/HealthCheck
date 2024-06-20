using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using HealthCheck.Configuration;

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
    internal sealed class HttpMonitor : HttpMonitorBase,  IHttpMonitor
    {
        private HttpProbeOptions? _httpProbeOptions;
        private ProbeLoggingOptions? _probeLoggingOptions;

        public HttpMonitor(ILogger<HttpMonitor> logger,
                           IHealthCheckService healthCheckService)
            : base(logger, healthCheckService)
        {
        }

        public async Task Monitor(HttpProbeOptions probeOptions, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            _httpProbeOptions = probeOptions;
            _probeLoggingOptions = loggingOptions;

            // Yield so that caller can continue processing
            await Task.Yield();

            try
            {
                await Execute(probeOptions.Endpoints, loggingOptions, cancellationToken);
            }

            // This would occur by cancellationToken if sub methods would use the ThrowIfCancelled
            catch (OperationCanceledException ex)
            {
                this.Logger.LogError(ex, "Http Probe Service shutting down by request");
            }


            // Any other exception
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Http Probe Service encountered an error and is shutting down");
            }

            await Task.CompletedTask;
        }


        protected override TcpListener CreateTcpListener()
        {
            ArgumentNullException.ThrowIfNull(_httpProbeOptions?.Port, "Port");
            return new TcpListener(IPAddress.Any, _httpProbeOptions.Port.Value);
        }
    }
}
