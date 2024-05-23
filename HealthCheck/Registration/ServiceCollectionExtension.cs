using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthCheck.DefaultChecks;

namespace HealthCheck.Registration
{
    public static class ServiceCollectionExtension
    {
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<HealthCheckWorker>();

            return new HealthChecksBuilder(services)
                    .AddCheckStatus<StatusCheck>("Default Status Check")
                    .AddCheckStartup<StartupCheck>("Default Startup Check")
                    .AddCheckReadiness<ReadinessCheck>("Default Readiness Check")
                    .AddCheckLiveness<LivenessCheck>("Default Liveness Check");
        }
    }
}
