using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Configuration;
using HealthCheck.Services;
using HealthCheck.Listeners.Kubernetes;


namespace HealthCheck.Registration
{
    public static partial class HostBuilderExtension
    {
        /// <summary>
        /// Add Kubernetes Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder UseKubernetesListener(this IHostBuilder builder)
        {
            return builder.ConfigureServices(ConfigureKubernetesListener);
        }

        /// <summary>
        /// Add Kubernetes Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseKubernetesListener(this IHostBuilder builder, Action<KubernetesListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices(services =>
            {
                ConfigureKubernetesListener(services);    // Set custome services
                services.Configure<KubernetesListenerOptions>(options => configureOptions(options));      // Call delegate
            });
        }

        /// <summary>
        /// Add Kubernetes Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseKubernetesListener(this IHostBuilder builder, Action<HostBuilderContext, KubernetesListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices((context, services) =>
            {
                ConfigureKubernetesListener(services);    // Set custome services
                services.Configure<KubernetesListenerOptions>(options => configureOptions(context, options));      // Call delegate
            });
        }

        /// <summary>
        /// Add Kubernetes Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder UseKubernetesListener(this IHostApplicationBuilder builder)
        {
            ConfigureKubernetesListener(builder.Services);
            return builder;
        }

        /// <summary>
        /// Add Kubernetes Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder UseKubernetesListener(this IHostApplicationBuilder builder, Action<KubernetesListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            ConfigureKubernetesListener(builder.Services);
            builder.Services.Configure<KubernetesListenerOptions>(options => configureOptions(options));      // Call delegate
            return builder;
        }


        private static void ConfigureKubernetesListener(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckServiceProvider, HealthCheckServiceProvider>();
            services.AddHostedService<KubernetesListener>();
        }
    }
}