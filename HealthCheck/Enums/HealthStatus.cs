namespace HealthCheck
{
    /// <summary>
    /// Represents the reported status of a health check result.
    /// </summary>
    /// <remarks>
    /// A status of Unhealthy should be considered the default value for a failing health check. Application developers may configure a health check to report a different status as desired.
    /// </remarks>
    public enum HealthStatus : int
    {
        UnHealthy = 0,
        Degraded = 1,
        Healthy = 2,
    }
}
