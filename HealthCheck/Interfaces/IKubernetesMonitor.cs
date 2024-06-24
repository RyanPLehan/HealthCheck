﻿using System;
using System.Threading.Tasks;
using HealthCheck.Configuration;

namespace HealthCheck
{
    internal interface IKubernetesMonitor
    {
        Task Monitor(TcpProbeOptions probeOptions, ProbeLogOptions loggingOptions, CancellationToken cancellationToken);
    }
}