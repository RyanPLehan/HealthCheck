using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthCheck.Configuration;
using HealthCheck.DefaultChecks;
using HealthCheck.Services;
using Microsoft.Extensions.Configuration;


namespace HealthCheck.Registration
{
    public static class ServiceCollectionExtension
    {
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
        {
            var builder = new HealthChecksBuilder(services)
                            .AddCheckStatus<StatusCheck>("Default Status Check")
                            .AddCheckStartup<StartupCheck>("Default Startup Check")
                            .AddCheckReadiness<ReadinessCheck>("Default Readiness Check")
                            .AddCheckLiveness<LivenessCheck>("Default Liveness Check");

            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<HealthCheckServer>();

            return builder;
        }

        /// <summary>
        /// Use the HTTP Monitor to respond to requests for Startup, Readiness and Liveness Health Check Probes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static IServiceCollection UseHttpMonitor(this IServiceCollection services)
        {
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.TryAddSingleton<IHttpProbeService, HttpProbeService>();
            return services;
        }

        /// <summary>
        /// Use the HTTPS Monitor to respond to requests for Startup, Readiness and Liveness Health Check Probes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static IServiceCollection UseHttpsMonitor(this IServiceCollection services)
        {
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.TryAddSingleton<IHttpsProbeService, HttpsProbeService>();
            return services;
        }


        /// <summary>
        /// Use the TCP Monitor to listen for Startup, Readiness and Liveness Health Check Probes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="startupPort">TCP Port number for Startup Probe.  Set to null to not monitor for Startup probe.</param>
        /// <param name="readinessPort">TCP Port number for Readiness Probe.  Set to null to not monitor for Readiness probe.</param>
        /// <param name="livenessPort">TCP Port number for Liveness Probe.  Set to null to not monitor for Liveness probe.</param>
        /// <returns></returns>
        public static IServiceCollection UseTcpMonitor(this IServiceCollection services)
        {
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.TryAddSingleton<ITcpProbeService, TcpProbeService>();
            return services;
        }


        /*
        /// <summary>
        /// Add Health Checks
        /// </summary>
        /// <param name="services"></param>
        /// <param name="namedConfigurationSection">
        /// <see cref="https://learn.microsoft.com/en-us/dotnet/core/extensions/options-library-authors#iconfiguration-parameter"/> 
        /// </param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services, IConfigurationSection namedConfigurationSection)
        {
            // Explicity Set Configuration from named configutation section passed in
            // Any one of these will work
            //services.AddOptions<HealthCheckOptions>().BindConfiguration(HealthCheckOptionsBuilder.CONFIGURATION_SECTION);     
            //services.AddOptions().Configure<HealthCheckOptions>(configuration.GetSection(HealthCheckOptionsBuilder.CONFIGURATION_SECTION))
            //services.Configure<HealthCheckOptions>(configuration.GetSection(HealthCheckOptionsBuilder.CONFIGURATION_SECTION));
            //services.AddOptions().Configure<HealthCheckOptions>(namedConfigurationSection);            
            //services.Configure<HealthCheckOptions>(namedConfigurationSection);

            bool isValid = (namedConfigurationSection.GetChildren().Count() > 0);

            services.AddOptions<HealthCheckOptions>()
                    .Bind(namedConfigurationSection)
                    .Validate((_) => isValid)
                    .ValidateOnStart();

            return AddHealthChecks(services);
        }
        */
    }
}
