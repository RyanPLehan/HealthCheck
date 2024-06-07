using Microsoft.Extensions.DependencyInjection;
using HealthCheck.Registration;

namespace HealthCheck.Tests.TcpProbe
{
    [TestClass]
    public class DefaultCheckTest
    {
        private readonly Program _program;

        public DefaultCheckTest()
        {
            _program = new Program().ConfigureServices(ConfigureServices);
            _program.Initialize();
        }

        [TestInitialize]
        public void Initialize()
        { }


        [TestCleanup]
        public void Cleanup()
        { }

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            int i = 10;
            // Act
            i++;
            // Assert
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register Health Check Worker and Default Checks
            services.AddHealthChecks();     
        }
    }
}