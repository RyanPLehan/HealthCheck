using Microsoft.Extensions.DependencyInjection;
using HealthCheck.Registration;

namespace HealthCheck.Tests.TcpProbe
{
    [TestClass]
    public class DefaultCheckTest
    {
        private static Program _program;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext testContext)
        {
            _program = new Program().ConfigureServices(services => services.AddHealthChecks());
            await _program.Initialize();
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _program.CleanUp();
        }


        [TestInitialize]
        public void TestInitialize()
        { }

        [TestCleanup]
        public void TestCleanup()
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

        [TestMethod]
        public void TestMethod2()
        {
            // Arrange
            int i = 10;
            // Act
            i++;
            // Assert
        }
    }
}