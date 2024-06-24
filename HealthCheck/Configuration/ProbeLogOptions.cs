using System;


namespace HealthCheck.Configuration
{
    public class ProbeLogOptions
    {
        // TODO: Use Callback function to determine when to log based upon health status
        public bool LogProbe { get; set; } = true;
        public bool LogWhenHealthy { get; set; } = false;
        public bool LogWhenDegraded { get; set; } = true;
        public bool LogWhenNotHealthy { get; set; } = true;
    }
}
