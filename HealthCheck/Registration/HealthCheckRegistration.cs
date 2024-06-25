using System;


namespace HealthCheck.Registration
{
    internal class HealthCheckRegistration
    {
        public Type Type { get; init; }
        public string Name { get; init; }
        public IEnumerable<string> Tags { get; init; } = Enumerable.Empty<string>();
    }
}
