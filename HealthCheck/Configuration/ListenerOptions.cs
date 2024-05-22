using System;

namespace HealthCheck.Configuration
{
    public class ListenerOptions
    {
        public HealthCheckProbeType HealthCheckProbeType { get; set; } = HealthCheckProbeType.HttpStatus;
        public int Port { get; set; } = 80;
        public string EndPoint { get; set; } = "healthcheck";
    }
}
