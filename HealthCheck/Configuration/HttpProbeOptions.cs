using HealthCheck;
using System;

namespace HealthCheck.Configuration
{
    public class HttpProbeOptions
    {
        public int Port { get; set; } = 8080;
        public EndpointAssignment Endpoints { get; set; }
    }

    public class EndpointAssignment
    {
        public string? Status { get; set; }
        public string? Startup { get; set; }
        public string? Readiness { get; set; }
        public string? Liveness { get; set; }

        internal IDictionary<HealthCheckType, string> ToDictionary()
        {
            IDictionary<HealthCheckType, string> ret = new Dictionary<HealthCheckType, string>();

            if (!String.IsNullOrWhiteSpace(Status))
                ret.Add(HealthCheckType.Status, Status);

            if (!String.IsNullOrWhiteSpace(Startup))
                ret.Add(HealthCheckType.Startup, Startup);

            if (!String.IsNullOrWhiteSpace(Readiness))
                ret.Add(HealthCheckType.Readiness, Readiness);

            if (!String.IsNullOrWhiteSpace(Liveness))
                ret.Add(HealthCheckType.Liveness, Liveness);

            return ret;
        }
    }
}
