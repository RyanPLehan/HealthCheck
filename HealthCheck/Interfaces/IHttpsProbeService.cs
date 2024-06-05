using System;
using System.Threading.Tasks;
using HealthCheck.Configuration;

namespace HealthCheck
{
    internal interface IHttpsProbeService
    {
        Task Monitor(HttpProbeOptions probeOptions, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken);
    }
}
