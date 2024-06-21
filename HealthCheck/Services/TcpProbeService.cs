using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using HealthCheck.Configuration;


namespace HealthCheck.Services
{
    /// <summary>
    /// This will respond to TCP probes using the given port
    /// </summary>
    /// <remarks>
    /// TCP Probes are just the opposite of the Request/Response model...
    /// 1. A TCP probe is successful if a connection is made.
    /// 2. TCP Listener must be activated AFTER the execution of the health checks AND have a healthy result
    /// 3. Only listen once then drop the connection
    /// 
    /// *** PLease Read the following ***
    /// Kuberentes has a Startup, Readiness, and Liveness Probe (in order)
    /// Therefore, if defined, the monitoring will not progress until the Probe has occurred.
    /// Meaning, if Startup, Readiness and Liveness are defined to be monitored.
    /// Then the monitor will not respond to the Readiness probe until the Startup Probe has occurred.
    /// The same applies to Liveness, in that, the monitor will not respond to the Liveness probe until the Readiness probe has occurred
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
            // Yield so that caller can continue processing
            await Task.Yield();

            // Process in order of Startup, Readiness, then Liveness
            // Must wait for probe before going on to next one
            if (probeOptions.Ports.Startup != null)
            {
                // One Time Monitoring
                await ExecuteIntervalCheck(probeOptions.CheckRetryIntervalInSeconds, HealthCheckType.Startup, loggingOptions, cancellationToken);
                await MonitorPort(probeOptions.Ports.Startup.Value, HealthCheckType.Startup, loggingOptions, cancellationToken);
            }

            if (probeOptions.Ports.Readiness != null)
            {
                // One Time Monitoring
                await ExecuteIntervalCheck(probeOptions.CheckRetryIntervalInSeconds, HealthCheckType.Readiness, loggingOptions, cancellationToken);
                await MonitorPort(probeOptions.Ports.Readiness.Value, HealthCheckType.Readiness, loggingOptions, cancellationToken);
            }

            if (probeOptions.Ports.Liveness != null)
            {
                // Keep monitoring for duration of application
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ExecuteIntervalCheck(probeOptions.CheckRetryIntervalInSeconds, HealthCheckType.Liveness, loggingOptions, cancellationToken);
                    await MonitorPort(probeOptions.Ports.Liveness.Value, HealthCheckType.Liveness, loggingOptions, cancellationToken);
                }
            }

            await Task.CompletedTask;
        }


        private async Task<HealthReport> ExecuteIntervalCheck(byte intervalInSeconds, HealthCheckType healthCheckType, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            int intervalTimeInMS = intervalInSeconds * 1000;        // Convert from seconds to milliseconds
            HealthReport healthReport;

            // Loop until the overall status is healthy or Degraded
            do
            {
                healthReport = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
                LoggingService.LogHealthCheck(_logger, loggingOptions, healthReport);

                if (healthReport.Status != HealthStatus.UnHealthy)
                    await Task.Delay(intervalTimeInMS);

            } while (!cancellationToken.IsCancellationRequested && healthReport.Status == HealthStatus.UnHealthy);

            return healthReport;
        }


        private async Task MonitorPort(int port, HealthCheckType healthCheckType, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Listen for probe on specified port.
                using (TcpListener listener = new TcpListener(IPAddress.Any, port))
                {
                    listener.Start();

                    // Accept and close to acknowledge
                    using TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                    LoggingService.LogProbe(_logger, loggingOptions, healthCheckType);
                }
            }

            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "TCP Probe Service shutting down by request");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP Probe Service listener encountered an error and is shutting down");
            }

            await Task.CompletedTask;
        }

    }
}
