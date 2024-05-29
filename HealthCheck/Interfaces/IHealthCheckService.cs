using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck
{
    public interface IHealthCheckService
    {
        Task<HealthCheckOverallStatus> CheckStatus(CancellationToken cancellationToken);
        Task<HealthCheckOverallStatus> CheckStartup(CancellationToken cancellationToken);
        Task<HealthCheckOverallStatus> CheckReadiness(CancellationToken cancellationToken);
        Task<HealthCheckOverallStatus> CheckLiveness(CancellationToken cancellationToken);

        internal T GetProbeService<T>();
        internal Task<HealthCheckOverallStatus> ExecuteCheckServices(HealthCheckType healthCheckType, CancellationToken cancellationToken);
    }
}
