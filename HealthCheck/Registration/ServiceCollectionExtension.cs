using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthCheck.Registration
{
    public static class ServiceCollectionExtension
    {
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<HealthCheckWorker>();

            return new HealthChecksBuilder(services);
        }
    }
}
