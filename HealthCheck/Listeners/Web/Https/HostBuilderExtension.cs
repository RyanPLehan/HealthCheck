using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Services;
using HealthCheck.Listeners.Web.Https;


namespace HealthCheck.HealthCheck.Registration
{
    public static partial class HostBuilderExtension
    {
        /// <summary>
        /// Add HTTPS Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder UseHttpsListener(this IHostBuilder builder)
        {
            return builder.ConfigureServices(ConfigureHttpListener);
        }

        /// <summary>
        /// Add HTTPS Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseHttpsListener(this IHostBuilder builder, Action<HttpsListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices(services =>
            {
                ConfigureHttpListener(services);    // Set custome services
                services.Configure<HttpsListenerOptions>(options => configureOptions(options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTPS Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder UseHttpsListener(this IHostBuilder builder, Action<HostBuilderContext, HttpsListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices((context, services) =>
            {
                ConfigureHttpListener(services);    // Set custome services
                services.Configure<HttpsListenerOptions>(options => configureOptions(context, options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTPS Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder UseHttpsListener(this IHostApplicationBuilder builder)
        {
            ConfigureHttpListener(builder.Services);
            return builder;
        }

        /// <summary>
        /// Add HTTPS Listener
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder UseHttpsListener(this IHostApplicationBuilder builder, Action<HttpsListenerOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            ConfigureHttpListener(builder.Services);
            builder.Services.Configure<HttpsListenerOptions>(options => configureOptions(options));      // Call delegate
            return builder;
        }


        private static void ConfigureHttpListener(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckServiceProvider, HealthCheckServiceProvider>();
            services.AddHostedService<HttpsListener>();
        }
    }
}