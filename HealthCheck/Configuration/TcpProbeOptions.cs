using System;

namespace HealthCheck.Configuration
{
    public class TcpProbeOptions : ProbeOptions
    {
        public TcpProbeOptions()
        {
            // Set Default Port
            this.Port = 8081;
            this.HealthCheckProbeType = HealthCheckProbeType.Tcp;
        }
    }
}
