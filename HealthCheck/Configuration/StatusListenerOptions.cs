using System;


namespace HealthCheck.Configuration
{
    public class StatusListenerOptions
    {
        public int Port { get; set; } = 80;
        public string EndPoint { get; set; } = "healthcheck";


        public static explicit operator ListenerOptions?(StatusListenerOptions options)
        {
            ListenerOptions? ret = null;
            if (options != null)
            {
                ret = new ListenerOptions()
                {
                    HealthCheckProbeType = HealthCheckProbeType.Http,
                    Port = options.Port,
                    EndPoint = options.EndPoint,
                };
            }

            return ret;
        }
    }
}
