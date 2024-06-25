using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using HealthCheck.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


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
    internal class KubernetesMonitor : BackgroundService
    {
        private readonly ILogger<KubernetesMonitor> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ProbeLogOptions _probeLogOptions;
        private readonly KubernetesMonitorOptions _monitorOptions;

        private enum ProbeTypeEnum : int
        {
            Startup = 0,
            Readiness = 1,
            Liveness = 2,
        };


        public KubernetesMonitor(ILogger<KubernetesMonitor> logger,
                                 IHealthCheckService healthCheckService,
                                 IOptions<ProbeLogOptions> probeLogOptions,
                                 IOptions<KubernetesMonitorOptions> monitorOptions)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));
            ArgumentNullException.ThrowIfNull(probeLogOptions?.Value, nameof(probeLogOptions));
            ArgumentNullException.ThrowIfNull(monitorOptions?.Value, nameof(monitorOptions));

            _logger = logger;
            _healthCheckService = healthCheckService;
            _probeLogOptions = probeLogOptions.Value;
            _monitorOptions = monitorOptions.Value;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Task task;
            try
            {
                _logger.LogInformation("Starting: Monitor");
                task = base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Monitor failed to start");
                task = Task.FromException(ex);
            }

            return task;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping: Monitor");
            return base.StopAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Process in order of Startup, Readiness, then Liveness
            // Must wait for probe before going on to next one
            if (_monitorOptions.Startup != null)
            {
                // One Time Monitoring
                await ExecuteIntervalCheck(_monitorOptions.CheckRetryIntervalInSeconds, ProbeTypeEnum.Startup, cancellationToken);
                await MonitorPort(_monitorOptions.Startup.Port, ProbeTypeEnum.Startup, cancellationToken);
            }

            if (_monitorOptions.Readiness != null)
            {
                // One Time Monitoring
                await ExecuteIntervalCheck(_monitorOptions.CheckRetryIntervalInSeconds, ProbeTypeEnum.Readiness, cancellationToken);
                await MonitorPort(_monitorOptions.Readiness.Port, ProbeTypeEnum.Readiness, cancellationToken);
            }

            if (_monitorOptions.Liveness != null)
            {
                // Keep monitoring for duration of application
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ExecuteIntervalCheck(_monitorOptions.CheckRetryIntervalInSeconds, ProbeTypeEnum.Liveness, cancellationToken);
                    await MonitorPort(_monitorOptions.Liveness.Port, ProbeTypeEnum.Liveness, cancellationToken);
                }
            }

            await Task.CompletedTask;
        }


        private async Task<HealthReport> ExecuteIntervalCheck(byte intervalInSeconds, ProbeTypeEnum probeType, CancellationToken cancellationToken)
        {
            int intervalTimeInMS = intervalInSeconds * 1000;        // Convert from seconds to milliseconds
            HealthReport healthReport;

            // Loop until the overall status is healthy or Degraded
            do
            {
                healthReport = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
                LoggingService.LogHealthCheck(_logger, _probeLogOptions, healthReport);

                if (healthReport.Status == HealthStatus.UnHealthy)
                    await Task.Delay(intervalTimeInMS);

            } while (!cancellationToken.IsCancellationRequested && healthReport.Status == HealthStatus.UnHealthy);

            return healthReport;
        }


        private async Task MonitorPort(int port, ProbeTypeEnum probeType, CancellationToken cancellationToken)
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
                    LoggingService.LogProbe(_logger, _probeLogOptions, probeType.ToString());
                }
            }

            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Monitor shutting down by request");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Monitor encountered an error and is shutting down");
            }

            await Task.CompletedTask;
        }

    }
}
