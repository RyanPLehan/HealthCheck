using System;


namespace HealthCheck.Configuration
{
    public class ProbeLoggingOptions
    {
        public bool LogProbe { get; set; } = true;
        public bool LogWhenHealthy { get; set; } = false;
        public bool LogWhenDegraded { get; set; } = true;
        public bool LogWhenNotHealthy { get; set; } = true;
    }
}
