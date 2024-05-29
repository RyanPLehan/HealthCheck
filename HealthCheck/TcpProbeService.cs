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
    internal class TcpProbeService : ITcpProbeService
    {
        private readonly ILogger<TcpProbeService> _logger;
        private readonly IHealthCheckService _healthCheckService;


        public TcpProbeService(ILogger<TcpProbeService> logger,
                               IHealthCheckService healthCheckService)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));

            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        public async Task Monitor(TcpProbeOptions probeOptions, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            // Process in order of Startup, Readiness, then Liveness
            // Must wait for probe before going on to next one
            if (probeOptions.Ports.Startup != null)
                await MonitorOnce(probeOptions.Ports.Startup.Value, HealthCheckType.Startup, loggingOptions, cancellationToken);

            if (probeOptions.Ports.Readiness != null)
                await MonitorOnce(probeOptions.Ports.Readiness.Value, HealthCheckType.Readiness, loggingOptions, cancellationToken);

            if (probeOptions.Ports.Liveness != null)
                await MonitorContinuously(probeOptions.Ports.Liveness.Value, HealthCheckType.Liveness, loggingOptions, cancellationToken);

            await Task.CompletedTask;
        }


        private async Task MonitorOnce(int port, HealthCheckType healthCheckType, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            int intervalTimeInMS = 3000;                        // 3 Seconds
            HealthCheckOverallStatus healthCheckOverallStatus;

            try
            {
                // Loop until the overall status is healthy
                do
                {
                    healthCheckOverallStatus = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
                    LogHealthCheck(loggingOptions, healthCheckOverallStatus);

                    if (healthCheckOverallStatus.HealthStatus != HealthStatus.Healthy)
                        await Task.Delay(intervalTimeInMS);

                } while (!cancellationToken.IsCancellationRequested && healthCheckOverallStatus.HealthStatus != HealthStatus.Healthy);

    
                // Listen for probe on specified port.
                using (TcpListener listener = new TcpListener(IPAddress.Any, port))
                {
                    listener.Start();

                    // Accept and close to acknowledge
                    using TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                    LogProbe(loggingOptions, healthCheckType);
                }
            }

            catch (OperationCanceledException ex)
            { }

            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP Probe Service encountered an error and is shutting down");
            }

            await Task.CompletedTask;
        }





        private async Task MonitorContinuously(int port, HealthCheckType healthCheckType, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            try
            {
                using (TcpListener listener = new TcpListener(IPAddress.Any, port))
                {
                    listener.Start();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        using (TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken))
                        {
                            try
                            {
                                if (healthCheckType != HealthCheckType.Unknown)
                                {
                                    LogProbe(loggingOptions, healthCheckType);
                                    HealthCheckOverallStatus healthCheckOverallStatus = await ProcessHealthCheck(client, healthCheckType, cancellationToken);
                                    LogHealthCheck(loggingOptions, healthCheckOverallStatus);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Tcp Probe Service failed when processing request");
                            }
                        }
                    }
                }
            }

            catch (OperationCanceledException ex)
            { }

            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP Probe Service encountered an error and is shutting down");
            }


            await Task.CompletedTask;
        }


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
