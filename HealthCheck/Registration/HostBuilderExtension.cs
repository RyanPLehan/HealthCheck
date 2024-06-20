﻿using System;
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
            builder.ConfigureServices(services =>
            {
                services.AddMemoryCache();
                services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
                services.AddHostedService<HealthCheckMonitor>();
            });

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
            builder.Services.AddMemoryCache();
            builder.Services.TryAddSingleton<IHealthCheckService, HealthCheckService>();
            builder.Services.AddHostedService<HealthCheckMonitor>();

            return builder;
        }
    }
}