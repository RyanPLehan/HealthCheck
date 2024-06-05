using HealthCheck;
using System;

namespace HealthCheck.Configuration
{
    public class HttpProbeOptions
    {
        public int? Port { get; set; } = null;
        public int? SslPort { get; set; } = null;
        public EndpointAssignment Endpoints { get; set; } = new EndpointAssignment();
    }

    public class EndpointAssignment
    {
        public string? Status { get; set; }
        public string? Startup { get; set; }
        public string? Readiness { get; set; }
        public string? Liveness { get; set; }
    }
}
