using System;


namespace HealthCheck.Configuration
{
    public class HealthCheckOptions
    {
        ListenerOptions Status{ get; set; }
        ListenerOptions Startup { get; set; }
        ListenerOptions Readiness { get; set; }
        ListenerOptions Liveness { get; set; }
    }
}
