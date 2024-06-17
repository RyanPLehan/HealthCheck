using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.DefaultChecks
{
    /// <summary>
    /// Default check for Status probe
    /// </summary>
    /// <remarks>
    /// According to Microsoft documentation, it should return Unhealthy by default
    /// Any other health check result should come from developer
    /// </remarks>
    internal class StatusCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {            
            return Task.FromResult(HealthCheckResult.Unhealthy("In compliance with Microsoft, by default, always unhealthy"));
        }
    }
}
