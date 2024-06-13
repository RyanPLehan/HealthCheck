using System;

namespace HealthCheck.Tests.HttpProbe
{
    internal class HealthCheckResults
    {
        public string OverallStatus { get; set; }

        public IEnumerable<KeyValuePair<string, string>> HealthChecks { get; set; }
    }
}
