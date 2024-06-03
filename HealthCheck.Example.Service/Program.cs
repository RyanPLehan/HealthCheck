using HealthCheck.Registration;
using HealthCheck.Example.Service.HealthChecks;

namespace HealthCheck.Example.Service
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
            return services;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()     // Auto adds default checks
                    .AddCheckStartup<SystemCheck>("Preflight System Check")
                    .AddCheckReadiness<DatabaseCheck>("Database Check")
                    .AddCheckLiveness<DatabaseCheck>("Database Check")
                    .AddCheckLiveness<ApiCheck>("Internal API Check")
                    .AddCheckLiveness<ServerPingCheck>("File Server Ping Check")
                    .AddCheckStatus<SystemCheck>("Preflight System Check")
                    .AddCheckStatus<DatabaseCheck>("Database Check")
                    .AddCheckStatus<ApiCheck>("Internal API Check")
                    .AddCheckStatus<ServerPingCheck>("File Server Ping Check");

            return services;
        }

        private static IServiceCollection ConfigureWorkers(IServiceCollection services)
        {
            services.AddHostedService<Worker>();
            return services;
        }
    }
}