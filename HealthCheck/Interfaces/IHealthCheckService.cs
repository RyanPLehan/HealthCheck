using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck
{
    public interface IHealthCheckService
    {
        Task<IEnumerable<KeyValuePair<string,HealthCheckResult>>> CheckStatus(CancellationToken cancellationToken);
        Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckStartup(CancellationToken cancellationToken);
        Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckReadiness(CancellationToken cancellationToken);
        Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckLiveness(CancellationToken cancellationToken);
    }
}
