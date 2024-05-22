using System;

namespace HealthCheck.Configuration
{
    public class ProbeListenerOptions
    {
        public HealthCheckProbeType HealthCheckProbeType { get; set; }
        public int Port { get; set; }
        public string EndPoint { get; set; } = "//";


        public static explicit operator ListenerOptions?(ProbeListenerOptions options)
        {
            ListenerOptions? ret = null;
            if (options != null)
            {
                ret = new ListenerOptions()
                       {
                           HealthCheckProbeType = options.HealthCheckProbeType,
                           Port = options.Port,
                           EndPoint = options.EndPoint,
                       };
            }

            return ret;
        }
    }
}
