using System;

namespace HealthCheck.Configuration
{
    public class HealthCheckOptions
    {
        public ProbeLoggingOptions Logging { get; set; } = new ProbeLoggingOptions();
        public HttpProbeOptions HttpProbe { get; set; }
        public TcpProbeOptions TcpProbe { get; set; }
    }
}
