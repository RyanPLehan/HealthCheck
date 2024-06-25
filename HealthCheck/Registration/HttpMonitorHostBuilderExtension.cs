using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Configuration;
using HealthCheck.Services;


namespace HealthCheck.Registration
{
    public static class HttpMonitorHostBuilderExtension
    {
        /// <summary>
        /// Add HTTP Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder AddHttpMonitor(this IHostBuilder builder)
        {
            return builder.ConfigureServices(ConfigureServices);
        }

        /// <summary>
        /// Add HTTP Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder AddHttpMonitor(this IHostBuilder builder, Action<HttpMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices(services =>
            {
                ConfigureServices(services);    // Set custome services
                services.Configure<HttpMonitorOptions>(options => configureOptions(options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTP Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostBuilder AddHttpMonitor(this IHostBuilder builder, Action<HostBuilderContext, HttpMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            return builder.ConfigureServices((context, services) =>
            {
                ConfigureServices(services);    // Set custome services
                services.Configure<HttpMonitorOptions>(options => configureOptions(context, options));      // Call delegate
            });
        }

        /// <summary>
        /// Add HTTP Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder AddHttpMonitor(this IHostApplicationBuilder builder)
        {
            ConfigureServices(builder.Services);
            return builder;
        }

        /// <summary>
        /// Add HTTP Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder AddHttpMonitor(this IHostApplicationBuilder builder, Action<HttpMonitorOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            ConfigureServices(builder.Services);
            builder.Services.Configure<HttpMonitorOptions>(options => configureOptions(options));      // Call delegate
            return builder;
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddHostedService<HttpMonitor>();
        }
    }
}