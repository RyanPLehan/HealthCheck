using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.Registration
{
    internal class HealthCheckRegistration
    {
        public Type Type { get; init; }
        public string Name { get; init; }
        public HealthCheckType HealthCheckType { get; init; }
    }
}
