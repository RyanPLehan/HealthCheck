using HealthCheck;
using System;

namespace HealthCheck.Configuration
{
    public class HttpProbeOptions
    {
        public int Port { get; set; } = 8080;
        public EndpointAssignment Endpoints { get; set; }
    }

    public class EndpointAssignment
    {
        public string Status { get; set; } = "health/status";
        public string Startup { get; set; } = "health/startup";
        public string Readiness { get; set; } = "health/readiness";
        public string Liveness { get; set; } = "health/liveness";
    }
}
