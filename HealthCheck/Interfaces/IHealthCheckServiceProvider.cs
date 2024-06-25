using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck
{
    internal interface IHealthCheckServiceProvider
    {
        Task<HealthReport> CheckStatus(CancellationToken cancellationToken);

        T GetService<T>();
        Task<HealthReport> ExecuteCheckServices(CancellationToken cancellationToken);
    }
}
