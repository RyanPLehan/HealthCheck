using System;
using System.Threading.Tasks;
using HealthCheck;

namespace HealthCheck.Example.Service.HealthChecks
{
    /// <summary>
    /// Constantly ping server to make sure all is good
    /// </summary>
    internal class ServerPingCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {            
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
