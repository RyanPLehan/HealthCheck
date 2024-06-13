using HealthCheck.Registration;
using HealthCheck.Tests.HttpProbe;
using System.Net.Sockets;


namespace HealthCheck.Tests.TcpProbe
{
    [TestClass]
    public class DefaultChecksTest 
    {
        [ClassInitialize]
        public static async Task ClassInitialize(TestContext testContext)
        { }

        [ClassCleanup]
        public static async Task ClassCleanup()
        { }

        [TestInitialize]
        public void TestInitialize()
        { }

        [TestCleanup]
        public void TestCleanup()
        { }



        [TestMethod]
        [DataRow("Startup", 8081, 1)]
        [DataRow("Readiness", 8081, 1)]
        [DataRow("Liveness", 8081, 3)]
        public async Task TestPortMonitor(string monitor, int port, int iterationCount)
        {
            // Arrange
            for (int i = 0; i < iterationCount; i++)
            {
                // Act
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(UrlBuilder.Host, port);
                }

                await Task.Delay(250);      // Simulate a delay
            }

            // Assert
            // No Errors
        }
    }
}