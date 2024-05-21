using Microsoft.Extensions.DependencyInjection.Extensions;
using System;


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
