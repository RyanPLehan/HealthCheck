using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HealthCheck.Configuration;
using HealthCheck.Services;


namespace HealthCheck.Registration
{
    public static class HostBuilderExtension
    {
        /// <summary>
        /// Add Health Check Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Kept for backwards compatiblity for apps using IHostBuilder that use callbacks
        /// </remarks>
        public static IHostBuilder UseHealthCheckMonitor(this IHostBuilder builder)
        {
            builder.ConfigureServices(ConfigureServices);
            return builder;
        }


        /// <summary>
        /// Add Health Check Monitor
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>
        /// Update for more modern approach that uses more linear coding style
        /// </remarks>
        public static IHostApplicationBuilder UseHealthCheckMonitor(this IHostApplicationBuilder builder)
        {
            ConfigureServices(builder.Services);
            return builder;
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IHealthCheckService, HealthCheckService>();

            services.TryAddSingleton<IHttpMonitor, HttpMonitor>();
            services.TryAddSingleton<IHttpsMonitor, HttpsMonitor>();
            services.TryAddSingleton<ITcpMonitor, TcpMonitor>();

            services.AddHostedService<HealthCheckMonitor>();

        }
    }
}
