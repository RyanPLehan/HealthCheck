using System;

namespace HealthCheck.Configuration
{
    public class TcpProbeOptions
    {
        public PortAssignment Ports { get; set; }
    }

    public class PortAssignment
    {
        public int? Startup { get; set; } = null;
        public int? Readiness { get; set; } = null;
        public int? Liveness { get; set; } = null;
    }
}
