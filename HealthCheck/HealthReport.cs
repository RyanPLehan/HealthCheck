using System;
using System.Collections.Generic;

namespace HealthCheck
{
    public class HealthReport
    {
        internal HealthReport(HealthCheckType healthCheckType, 
                              HealthStatus overallHealthStatus,
                              IEnumerable<KeyValuePair<string, HealthCheckResult>> entries)
        {
            this.HealthCheckType = healthCheckType;
            this.Status = overallHealthStatus;
            this.Entries = entries;
        }

        /// <summary>
        /// Gets the health check type 
        /// </summary>
        public HealthCheckType HealthCheckType { get; private init; }

        /// <summary>
        /// Overall Health Status
        /// </summary>
        public HealthStatus Status { get; private init; }

        /// <summary>
        /// Results where the Key is the name of the health check given by the developer
        /// </summary>
        public IEnumerable<KeyValuePair<string, HealthCheckResult>> Entries { get; private init; }
    }
}
