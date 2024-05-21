using System;

namespace HealthCheck
{
    public interface IHealthChecksBuilder
    {
        IServiceCollection Services { get; }

        IHealthChecksBuilder AddCheckStatus<TService>(string name) where TService : class, IHealthCheck;
        IHealthChecksBuilder AddCheckStatus(IHealthCheck instance, string name);

        IHealthChecksBuilder AddCheckStartup<TService>(string name) where TService : class, IHealthCheck;
        IHealthChecksBuilder AddCheckStartup(IHealthCheck instance, string name);

        IHealthChecksBuilder AddCheckReadiness<TService>(string name) where TService : class, IHealthCheck;
        IHealthChecksBuilder AddCheckReadiness(IHealthCheck instance, string name);

        IHealthChecksBuilder AddCheckLiveness<TService>(string name) where TService : class, IHealthCheck;
        IHealthChecksBuilder AddCheckLiveness(IHealthCheck instance, string name);
    }
}
