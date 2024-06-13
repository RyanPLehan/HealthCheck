using System.Net.Http;
using HealthCheck.Registration;
using HealthCheck.Tests.Formatters;


namespace HealthCheck.Tests.HttpProbe
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
        [DataRow("health/status")]
        public async Task TestHttpStatusMonitor(string endpoint)
        {
            // Arrange
            Uri address = UrlBuilder.BuildHttp(endpoint);

            using (HttpClient client = new HttpClient())
            {
                // Act
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, address))
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        HealthCheckResults results = await GetResults(response);

                        // Assert
                        Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
                        Assert.IsTrue(results.OverallStatus.Equals("unhealthy", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
        }

        [TestMethod]
        [DataRow("health/status")]
        public async Task TestHttpsStatusMonitor(string endpoint)
        {
            // Arrange
            Uri address = UrlBuilder.BuildHttps(endpoint);

            using (HttpClient client = new HttpClient())
            {
                // Act
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, address))
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        HealthCheckResults results = await GetResults(response);

                        // Assert
                        Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
                        Assert.IsTrue(results.OverallStatus.Equals("unhealthy", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
        }



        [TestMethod]
        [DataRow("health/startup")]
        [DataRow("health/readiness")]
        [DataRow("health/liveness")]
        public async Task TestHttpPortMonitor(string endpoint)
        {
            // Arrange
            Uri address = UrlBuilder.BuildHttp(endpoint);

            using (HttpClient client = new HttpClient())
            {
                // Act
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, address))
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        // Assert
                        Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
                    }
                }
            }
        }


        [TestMethod]
        [DataRow("health/startup")]
        [DataRow("health/readiness")]
        [DataRow("health/liveness")]
        public async Task TestHttpsPortMonitor(string endpoint)
        {
            // Arrange
            Uri address = UrlBuilder.BuildHttps(endpoint);

            using (HttpClient client = new HttpClient())
            {
                // Act
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, address))
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        // Assert
                        Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
                    }
                }
            }
        }


        private async Task<HealthCheckResults> GetResults(HttpResponseMessage httpResponse)
        {
            string json = await httpResponse.Content.ReadAsStringAsync();
            return Json.Deserialize<HealthCheckResults>(json);
        }
    }
}