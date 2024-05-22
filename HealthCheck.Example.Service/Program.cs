using HealthCheck.Configuration;
using HealthCheck.Registration;

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
            // Explicity Set Configuration
            // services.Configure<HealthCheckOptions>(configuration.GetSection(HealthCheckWorker.CONFIGURATION_SECTION));
            return services;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.Add
            return services;
        }

        private static IServiceCollection ConfigureWorkers(IServiceCollection services)
        {
            return services;
        }
    }
}