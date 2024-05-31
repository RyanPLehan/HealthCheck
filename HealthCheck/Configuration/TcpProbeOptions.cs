using System;

namespace HealthCheck.Configuration
{
    public class TcpProbeOptions
    {
        public byte CheckRetryIntervalInSeconds { get; set; } = 3;
        public PortAssignment Ports { get; set; } = new PortAssignment();
    }

    public class PortAssignment
    {
        public int? Startup { get; set; } = null;
        public int? Readiness { get; set; } = null;
        public int? Liveness { get; set; } = null;
    }
}
