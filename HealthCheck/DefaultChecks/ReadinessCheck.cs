using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.DefaultChecks
{
    /// <summary>
    /// Default check for Readiness Probe
    /// </summary>
    /// <remarks>
    /// Will return Healthy by default.
    /// If other checks are needed, developer should add their own
    /// </remarks>
    internal class ReadinessCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {            
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
