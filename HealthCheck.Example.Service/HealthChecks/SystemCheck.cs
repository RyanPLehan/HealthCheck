using System;
using System.Threading.Tasks;
using HealthCheck;

namespace HealthCheck.Example.Service.HealthChecks
{
    /// <summary>
    /// Check Systems before ready to start processing
    /// </summary>
    internal class SystemCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {            
            // Check system before starting
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
