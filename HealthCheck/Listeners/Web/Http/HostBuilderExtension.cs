using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Services;
using HealthCheck.Listeners.Web.Http;


namespace HealthCheck.Registration
{
    public static partial class HostBuilderExtension
    {
        /// <summary>
        /// Add HTTP Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder UseHttpListener(this IHostBuilder builder)
        {
            return builder.ConfigureServices(ConfigureKubernetesListener);
        }

        /// <summary>
        /// Add HTTP Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseHttpListener(this IHostBuilder builder, Action<HttpListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices(services =>
            {
                ConfigureKubernetesListener(services);    // Set custome services
                services.Configure<HttpListenerOptions>(options => configureOptions(options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTP Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseHttpListener(this IHostBuilder builder, Action<HostBuilderContext, HttpListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices((context, services) =>
            {
                ConfigureKubernetesListener(services);    // Set custome services
                services.Configure<HttpListenerOptions>(options => configureOptions(context, options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTP Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder UseHttpListener(this IHostApplicationBuilder builder)
        {
            ConfigureKubernetesListener(builder.Services);
            return builder;
        }

        /// <summary>
        /// Add HTTP Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder UseHttpListener(this IHostApplicationBuilder builder, Action<HttpListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            ConfigureKubernetesListener(builder.Services);
            builder.Services.Configure<HttpListenerOptions>(options => configureOptions(options));      // Call delegate
            return builder;
        }


        private static void ConfigureHttpListener(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckServiceProvider, HealthCheckServiceProvider>();
            services.AddHostedService<HttpListener>();
        }
    }
}