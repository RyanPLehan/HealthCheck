using System;

namespace HealthCheck.Configuration
{
    public class HttpProbeOptions : ProbeOptions
    {
        public HttpProbeOptions()
        {
            // Set Default Port
            this.Port = 8080;
            this.HealthCheckProbeType = HealthCheckProbeType.Http;
        }

        public IDictionary<HealthCheckType, string> EndPoints { get; set; }
            = new Dictionary<HealthCheckType, string>()
            {
                {HealthCheckType.Status, "health/status"},
                {HealthCheckType.Startup, "health/startup"},
                {HealthCheckType.Readiness, "health/readiness"},
                {HealthCheckType.Liveness, "health/liveness"},
            };
    }
}
