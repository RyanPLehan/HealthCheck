using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Configuration;
using HealthCheck.Services;


namespace HealthCheck.Registration
{
    public static class KubernetesMonitorHostBuilderExtension
    {
        /// <summary>
        /// Add Kubernetes Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder UseKubernetesMonitor(this IHostBuilder builder)
        {
            return builder.ConfigureServices(ConfigureServices);
        }

        /// <summary>
        /// Add Kubernetes Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseKubernetesMonitor(this IHostBuilder builder, Action<KubernetesMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices(services =>
            {
                ConfigureServices(services);    // Set custome services
                services.Configure<KubernetesMonitorOptions>(options => configureOptions(options));      // Call delegate
            });
        }

        /// <summary>
        /// Add Kubernetes Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseKubernetesMonitor(this IHostBuilder builder, Action<HostBuilderContext, KubernetesMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices((context, services) =>
            {
                ConfigureServices(services);    // Set custome services
                services.Configure<KubernetesMonitorOptions>(options => configureOptions(context, options));      // Call delegate
            });
        }

        /// <summary>
        /// Add Kubernetes Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder UseKubernetesMonitor(this IHostApplicationBuilder builder)
        {
            ConfigureServices(builder.Services);
            return builder;
        }

        /// <summary>
        /// Add Kubernetes Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder UseKubernetesMonitor(this IHostApplicationBuilder builder, Action<KubernetesMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            ConfigureServices(builder.Services);
            builder.Services.Configure<KubernetesMonitorOptions>(options => configureOptions(options));      // Call delegate
            return builder;
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<KubernetesMonitor>();
        }
    }
}