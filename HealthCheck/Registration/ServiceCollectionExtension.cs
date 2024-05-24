using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthCheck.Configuration;
using HealthCheck.DefaultChecks;

namespace HealthCheck.Registration
{
    public static class ServiceCollectionExtension
    {
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
        {
            // Explicity Set Configuration
            // services.Configure<HealthCheckOptions>(configuration.GetSection(HealthCheckOptionsBuilder.CONFIGURATION_SECTION));

            var builder = new HealthChecksBuilder(services)
                            .AddCheckStatus<StatusCheck>("Default Status Check")
                            .AddCheckStartup<StartupCheck>("Default Startup Check")
                            .AddCheckReadiness<ReadinessCheck>("Default Readiness Check")
                            .AddCheckLiveness<LivenessCheck>("Default Liveness Check");

            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<HealthCheckWorker>();

            return builder;
        }
    }
}
