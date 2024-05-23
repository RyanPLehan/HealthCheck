using System;

namespace HealthCheck.Configuration
{
    public class ProbeOptions
    {
        public int Port { get; set; }
        internal HealthCheckProbeType HealthCheckProbeType { get; init; }
    }
}
