using System;
using HealthCheck.Configuration;
using HealthCheck.Formatters;
using Microsoft.Extensions.Logging;

namespace HealthCheck.Services
{
    internal static class LoggingService
    {

        public static void LogProbe(ILogger logger, ProbeLogOptions loggingOptions, string probe)
        {
            if (loggingOptions.LogProbe)
                logger.LogInformation("Health Check Probe: {0}", probe);
        }


        public static void LogHealthCheck(ILogger logger, ProbeLogOptions loggingOptions, HealthReport healthReport)
        {
            if (loggingOptions.LogWhenHealthy &&
                healthReport.Status == HealthStatus.Healthy)
            {
                logger.LogInformation("Health Check Result: {0}", healthReport.Status.ToString());
            }

            if (loggingOptions.LogWhenDegraded &&
                healthReport.Status == HealthStatus.Degraded)
            {
                logger.LogInformation("Health Check Result: {0}", healthReport.Status.ToString());
                logger.LogWarning("Health Check Detailed Results: {0}", Json.Serialize(healthReport));
            }


            if (loggingOptions.LogWhenNotHealthy &&
                healthReport.Status == HealthStatus.UnHealthy)
            {
                logger.LogWarning("Health Check Result: {0}", healthReport.Status.ToString());
                logger.LogWarning("Health Check Detailed Results: {0}", Json.Serialize(healthReport));
            }
        }

    }
}
