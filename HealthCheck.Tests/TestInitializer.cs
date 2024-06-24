using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HealthCheck.Registration;
using System.Data;
using Microsoft.Extensions.Internal;

namespace HealthCheck.Tests
{
    [TestClass]
    public class TestInitializer
    {
        private static IHost _host = null;

        public static TestContext TestContext { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IServiceCollection ServiceCollection { get; private set; }
        public static IConfigurationManager ConfigurationManager { get; private set; }

        [AssemblyInitialize]
        public static async Task Initialize(TestContext testContext)
        {
            if (_host != null)
                return;

            var builder = Host.CreateApplicationBuilder();

            // Setup Health Check Monitor
            builder.UseHealthCheckServer();

            ConfigureOptions(builder.Configuration, builder.Services);
            ConfigureServices(builder.Services);

            _host = builder.Build();

            TestContext = testContext;
            ConfigurationManager = builder.Configuration;
            ServiceCollection = builder.Services;
            ServiceProvider = _host.Services;

            _host.RunAsync();

            await Task.Delay(50);          // Delay to give time for host to start background service
        }

        [AssemblyCleanup]
        public static async Task CleanUp()
        {
            // Attempt a graceful shutdown
            if (_host != null)
            {
                await _host.StopAsync();
                _host = null;
            }
        }
        

        /// <summary>
        /// Need at least 1 Test Method for AssemblyInitialize to be called
        /// </summary>
        [TestMethod]
        public void TestInitialize()
        { }


        private static IServiceCollection ConfigureOptions(ConfigurationManager configuration, IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();     // Auto adds default checks
            return services;
        }
    }
}