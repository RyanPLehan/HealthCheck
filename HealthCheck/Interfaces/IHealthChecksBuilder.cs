using System;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCheck
{
    /// <summary>
    /// Mimic closely to MS HealthCheckBuilder
    /// <see cref="https://github.com/dotnet/aspnetcore/blob/main/src/HealthChecks/HealthChecks/src/DependencyInjection/HealthChecksBuilderAddCheckExtensions.cs"/>
    /// </summary>
    public interface IHealthChecksBuilder
    {
        IServiceCollection Services { get; }

        IHealthChecksBuilder AddCheck<TService>(string name) where TService : class, IHealthCheck;
        IHealthChecksBuilder AddCheck<TService>(string name, IEnumerable<string> tags) where TService : class, IHealthCheck;
        IHealthChecksBuilder AddCheck(IHealthCheck instance, string name);
        IHealthChecksBuilder AddCheck(IHealthCheck instance, string name, IEnumerable<string> tags);
        
        // default checks
        IHealthChecksBuilder AddDefaultHealthyCheck(string name);
        IHealthChecksBuilder AddDefaultHealthyCheck(string name, IEnumerable<string> tags);

        IHealthChecksBuilder AddDefaultDegradedCheck(string name);
        IHealthChecksBuilder AddDefaultDegradedCheck(string name, IEnumerable<string> tags);

        IHealthChecksBuilder AddDefaultUnhealthyCheck(string name);
        IHealthChecksBuilder AddDefaultUnhealthyCheck(string name, IEnumerable<string> tags);
    }
}
