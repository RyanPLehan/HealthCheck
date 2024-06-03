using System;
using System.Threading.Tasks;
using HealthCheck;

namespace HealthCheck.Example.Service.HealthChecks
{
    /// <summary>
    /// Check Database at all times
    /// </summary>
    internal class DatabaseCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {  
            // Do Database check
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
