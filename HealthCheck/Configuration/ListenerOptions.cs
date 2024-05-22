using System;

namespace HealthCheck.Configuration
{
    public class ListenerOptions
    {
        public HealthCheckProbeType HealthCheckProbeType { get; internal init; }
        public int Port { get; internal init; } 
        public string? EndPoint { get; internal init; }
    }
}
