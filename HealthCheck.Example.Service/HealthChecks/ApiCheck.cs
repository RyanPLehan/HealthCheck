using System;
using System.Threading.Tasks;
using HealthCheck;

namespace HealthCheck.Example.Service.HealthChecks
{
    /// <summary>
    /// Check APIs
    /// </summary>
    internal class ApiCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {            
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
