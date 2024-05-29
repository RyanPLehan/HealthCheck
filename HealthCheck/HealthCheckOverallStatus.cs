using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;


namespace HealthCheck
{
    public class HealthCheckOverallStatus
    {
        internal HealthCheckOverallStatus(HealthStatus healthStatus, IEnumerable<KeyValuePair<string, HealthCheckResult>> results)
        {
            this.HealthStatus = healthStatus;
            this.Results = results;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public HealthStatus HealthStatus { get; private init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public IEnumerable<KeyValuePair<string, HealthCheckResult>> Results { get; private init; }

        public string OverallStatus
        { get => this.HealthStatus.ToString(); }

        public IEnumerable<KeyValuePair<string, string>> HealthChecks
        { get => this.Results.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Status.ToString())); }
    }
}
