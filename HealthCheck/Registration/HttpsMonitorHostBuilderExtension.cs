using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Configuration;
using HealthCheck.Services;


namespace HealthCheck.Registration
{
    public static class HttpsMonitorHostBuilderExtension
    {
        /// <summary>
        /// Add HTTPS Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder AddHttpsMonitor(this IHostBuilder builder)
        {
            return builder.ConfigureServices(ConfigureServices);
        }

        /// <summary>
        /// Add HTTPS Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder AddHttpsMonitor(this IHostBuilder builder, Action<HttpsMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices(services =>
            {
                ConfigureServices(services);    // Set custome services
                services.Configure<HttpsMonitorOptions>(options => configureOptions(options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTPS Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder AddHttpsMonitor(this IHostBuilder builder, Action<HostBuilderContext, HttpsMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices((context, services) =>
            {
                ConfigureServices(services);    // Set custome services
                services.Configure<HttpsMonitorOptions>(options => configureOptions(context, options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTPS Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder AddHttpsMonitor(this IHostApplicationBuilder builder)
        {
            ConfigureServices(builder.Services);
            return builder;
        }

        /// <summary>
        /// Add HTTPS Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder AddHttpsMonitor(this IHostApplicationBuilder builder, Action<HttpsMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            ConfigureServices(builder.Services);
            builder.Services.Configure<HttpsMonitorOptions>(options => configureOptions(options));      // Call delegate
            return builder;
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<HttpsMonitor>();
        }
    }
}