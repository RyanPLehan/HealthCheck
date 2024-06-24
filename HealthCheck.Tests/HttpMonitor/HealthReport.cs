﻿using System;

namespace HealthCheck.Tests.HttpMonitor
{
    internal class HealthReport
    {
        public string Status { get; set; }

        public IEnumerable<KeyValuePair<string, string>> HealthChecks { get; set; }
    }
}