using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck
{
    public interface IHealthCheckService
    {
        Task<HealthCheckResults> CheckStatus(CancellationToken cancellationToken);
        Task<HealthCheckResults> CheckStartup(CancellationToken cancellationToken);
        Task<HealthCheckResults> CheckReadiness(CancellationToken cancellationToken);
        Task<HealthCheckResults> CheckLiveness(CancellationToken cancellationToken);

        internal T GetProbeService<T>();
        internal Task<HealthCheckResults> ExecuteCheckServices(HealthCheckType healthCheckType, CancellationToken cancellationToken);
    }
}
