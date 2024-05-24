
namespace HealthCheck
{
    public enum HealthCheckType: int
    {
        Status = 1,
        Startup = 2,
        Readiness = 3,
        Liveness = 4,
    }
}
