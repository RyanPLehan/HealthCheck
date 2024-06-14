using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck
{
    public interface IHealthCheckService
    {
        Task<HealthReport> CheckStatus(CancellationToken cancellationToken);
        Task<HealthReport> CheckStartup(CancellationToken cancellationToken);
        Task<HealthReport> CheckReadiness(CancellationToken cancellationToken);
        Task<HealthReport> CheckLiveness(CancellationToken cancellationToken);

        internal T GetProbeService<T>();
        internal Task<HealthReport> ExecuteCheckServices(HealthCheckType healthCheckType, CancellationToken cancellationToken);
    }
}
