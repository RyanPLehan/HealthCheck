using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HealthCheck.Registration;
using HealthCheck.Tests;

namespace HealthCheck.Tests
{
    internal class Program
    {
        private IServiceProvider _serviceProvider;
        private IServiceCollection _serviceCollection = null;
        private IConfigurationManager _configurationManager = null;
        private Action<IServiceCollection> _configureServicesFunction = null;
        private Action<IConfigurationManager, IServiceCollection> _configureOptions = null;

        public IServiceProvider ServiceProvider { get => _serviceProvider; }
        public IServiceCollection ServiceCollection { get => _serviceCollection; }
        public IConfigurationManager ConfigurationManager { get => _configurationManager;  }

        public Program ConfigureServices(Action<IServiceCollection> function)
        {
            ArgumentNullException.ThrowIfNull(function, nameof(function));
            _configureServicesFunction = function;
            return this;
        }

        public Program ConfigureOptions(Action<IConfigurationManager, IServiceCollection> function)
        {
            ArgumentNullException.ThrowIfNull(function, nameof(function));
            _configureOptions = function;
            return this;
        }


        public void Initialize()
        {
            var builder = Host.CreateApplicationBuilder();

            _configurationManager = builder.Configuration;
            _serviceCollection = builder.Services;

            ConfigureOptions(builder.Configuration, builder.Services);
            ConfigureServices(builder.Services);

            var host = builder.Build();
            host.Run();
        }

        private IServiceCollection ConfigureOptions(ConfigurationManager configuration, IServiceCollection services)
        {
            if (_configureOptions != null)
                _configureOptions(configuration, services);

            return services;
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            if (_configureServicesFunction != null)
                _configureServicesFunction(services);


            //services.AddHealthChecks()     // Auto adds default checks
            //        .AddCheckStartup<SystemCheck>("Preflight System Check")
            //        .AddCheckReadiness<DatabaseCheck>("Database Check")
            //        .AddCheckLiveness<DatabaseCheck>("Database Check")
            //        .AddCheckLiveness<ApiCheck>("Internal API Check")
            //        .AddCheckLiveness<ServerPingCheck>("File Server Ping Check")
            //        .AddCheckStatus<SystemCheck>("Preflight System Check")
            //        .AddCheckStatus<DatabaseCheck>("Database Check")
            //        .AddCheckStatus<ApiCheck>("Internal API Check")
            //        .AddCheckStatus<ServerPingCheck>("File Server Ping Check");

            return services;
        }
    }
}