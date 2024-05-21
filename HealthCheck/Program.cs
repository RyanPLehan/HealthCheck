using System;
using HealthCheck.Configuration;

namespace HealthCheck
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            ConfigureOptions(builder.Configuration, builder.Services);
            ConfigureServices(builder.Services);
            ConfigureWorkers(builder.Services);

            var host = builder.Build();
            host.Run();
        }

        private static IServiceCollection ConfigureOptions(ConfigurationManager configuration, IServiceCollection services)
        {
            services.Configure<HealthCheckOptions>(configuration.GetSection("HealthCheck"));
            return services;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection ConfigureWorkers(IServiceCollection services)
        {
            services.AddHostedService<HealthCheckWorker>();
            return services;
        }
    }
}