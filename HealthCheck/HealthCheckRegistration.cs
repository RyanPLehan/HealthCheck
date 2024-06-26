using System;


namespace HealthCheck
{
    public class HealthCheckRegistration
    {
        public Type Type { get; init; }
        public string Name { get; init; }
        public IEnumerable<string> Tags { get; init; } = Enumerable.Empty<string>();
    }
}
