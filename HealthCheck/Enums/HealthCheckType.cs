
namespace HealthCheck
{
    public enum HealthCheckType: int
    {
        Status = 0,
        Startup = 1,
        Readiness = 2,
        Liveness = 3,
    }
}
