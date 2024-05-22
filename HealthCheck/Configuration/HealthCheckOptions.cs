using System;


namespace HealthCheck.Configuration
{
    public class HealthCheckOptions
    {
        public StatusListenerOptions? Status { get; set; }
        public ProbeListenerOptions? Startup { get; set; }
        public ProbeListenerOptions? Readiness { get; set; }
        public ProbeListenerOptions? Liveness { get; set; }
    }
}
