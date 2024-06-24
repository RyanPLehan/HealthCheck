using HealthCheck.Registration;
using System.Net.Sockets;


namespace HealthCheck.Tests.KubernetesMonitor
{
    [TestClass]
    public class DefaultChecksTest 
    {
        private const string _host = "localhost";

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
                    await client.ConnectAsync(_host, port);
                }

                await Task.Delay(250);      // Simulate a delay
            }

            // Assert
            // No Errors
        }
    }
}