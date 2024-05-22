using System;
using HealthCheck.Extensions;

namespace HealthCheck.Configuration
{
    public class HealthCheckOptions
    {
        public StatusListenerOptions? Status { get; set; }
        public ProbeListenerOptions? Startup { get; set; }
        public ProbeListenerOptions? Readiness { get; set; }
        public ProbeListenerOptions? Liveness { get; set; }

        internal IEnumerable<ListenerOptions> Listeners
        {
            get
            {
                List<ListenerOptions> listeners = new List<ListenerOptions>();

                listeners.AddIfNotNull((ListenerOptions)this.Status);
                listeners.AddIfNotNull((ListenerOptions)this.Startup);
                listeners.AddIfNotNull((ListenerOptions)this.Readiness);
                listeners.AddIfNotNull((ListenerOptions)this.Liveness);

                return listeners;
            }
        }

    }
}
