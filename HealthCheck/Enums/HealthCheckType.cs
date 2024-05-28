
namespace HealthCheck
{
    internal enum HealthCheckType: int
    {
        Unknown = 0,
        Status = 1,
        Startup = 2,
        Readiness = 3,
        Liveness = 4,
    }
}
