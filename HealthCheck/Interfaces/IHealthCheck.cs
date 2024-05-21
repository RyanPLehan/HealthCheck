using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck
{
    public interface IHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken);
    }
}
