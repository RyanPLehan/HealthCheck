using System;
using HealthCheck.Configuration;
using HealthCheck.Formatters;
using Microsoft.Extensions.Logging;

namespace HealthCheck.Services
{
    internal static class LoggingService
    {

        public static void LogProbe(ILogger logger, ProbeLoggingOptions loggingOptions, HealthCheckType healthCheckType)
        {
            if (loggingOptions.LogProbe)
                logger.LogInformation("Health Check Probe: {0}", healthCheckType.ToString());
        }


        public static void LogHealthCheck(ILogger logger, ProbeLoggingOptions loggingOptions, HealthReport healthReport)
        {
            if (loggingOptions.LogStatusWhenHealthy &&
                healthReport.Status == HealthStatus.Healthy)
            {
                logger.LogInformation("Health Check Result: {0}", healthReport.Status.ToString());
            }

            if (loggingOptions.LogStatusWhenNotHealthy &&
                healthReport.Status != HealthStatus.Healthy)
            {
                logger.LogWarning("Health Check Result: {0}", healthReport.Status.ToString());
                logger.LogWarning("Health Check Detailed Results: {0}", Json.Serialize(healthReport));
            }
        }

    }
}
