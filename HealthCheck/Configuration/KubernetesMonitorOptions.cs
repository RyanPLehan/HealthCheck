using System;

namespace HealthCheck.Configuration
{
    public class KubernetesMonitorOptions
    {
        public byte CheckRetryIntervalInSeconds { get; set; } = 5;
        public KubernetesProbeOptions Startup { get; set; } = new KubernetesProbeOptions();
        public KubernetesProbeOptions Readiness { get; set; } = new KubernetesProbeOptions();
        public KubernetesProbeOptions Liveness { get; set; } = new KubernetesProbeOptions();
    }

    public class KubernetesProbeOptions
    {
        public int Port { get; set; } = 2310;
    }
}
