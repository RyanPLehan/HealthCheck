﻿using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json.Serialization;
using HealthCheck.Configuration;
using HealthCheck.Formatters;
using System.Threading;

namespace HealthCheck
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


        private async Task<HealthCheckResults> ExecuteIntervalCheck(byte intervalInSeconds, HealthCheckType healthCheckType, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            int intervalTimeInMS = intervalInSeconds * 1000;        // Convert from seconds to milliseconds
            HealthCheckResults healthCheckResults;

            // Loop until the overall status is healthy
            do
            {
                healthCheckResults = await _healthCheckService.ExecuteCheckServices(healthCheckType, cancellationToken);
                LogHealthCheck(loggingOptions, healthCheckResults);

                if (healthCheckResults.HealthStatus != HealthStatus.Healthy)
                    await Task.Delay(intervalTimeInMS);

            } while (!cancellationToken.IsCancellationRequested && healthCheckResults.HealthStatus != HealthStatus.Healthy);

            return healthCheckResults;
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
                    LogProbe(loggingOptions, healthCheckType);
                }
            }

            catch (OperationCanceledException)
            { }

            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP Probe Service listener encountered an error and is shutting down");
            }

            await Task.CompletedTask;
        }


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
