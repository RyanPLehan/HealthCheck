using System;
using System.Threading.Tasks;
using HealthCheck.Configuration;

namespace HealthCheck
{
    internal interface IHttpMonitor
    {
        Task Monitor(HttpProbeOptions probeOptions, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken);
    }
}
