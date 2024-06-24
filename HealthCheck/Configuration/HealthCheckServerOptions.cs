using System;

namespace HealthCheck.Configuration
{
    public class HealthCheckServerOptions
    {
        public int? HttpPort { get; set; } = 80;
        public int? HttpsPort { get; set; } = 443;
    }

    public class TcpPortAssignment
    {
        public int? Startup { get; set; } = 2310;
        public int? Readiness { get; set; } = 2310;
        public int? Liveness { get; set; } = 2310;
    }
}
