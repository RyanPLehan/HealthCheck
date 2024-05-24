using System;


namespace HealthCheck.Configuration
{
    public class ProbeLoggingOptions
    {
        public bool LogProbe { get; set; } = true;
        public bool LogStatusWhenHealthy { get; set; } = false;
        public bool LogStatusWhenNotHealthy { get; set; } = true;
    }
}
