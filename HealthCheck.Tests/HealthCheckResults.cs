using System;
using HealthCheck;

namespace HealthCheck.Tests
{
    internal class HealthCheckResults
    {
        public string OverallStatus { get; set; }

        public IEnumerable<KeyValuePair<string, string>> HealthChecks { get; set; }
    }
}
