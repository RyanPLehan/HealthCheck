using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using HealthCheck.Registration;


namespace HealthCheck
{
    /// <summary>
    /// This will execute the custom health check services
    /// </summary>
    internal class HealthCheckService : IHealthCheckService
    {
        private const string NO_REGISTRATION_NAME = "Unregistered Health Check Service";
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public HealthCheckService(ILogger<HealthCheckService> logger,
                                  IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            _logger = logger;
            _serviceProvider = serviceProvider;
        }


        public async Task<HealthCheckResults> CheckLiveness(CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckServices(HealthCheckType.Liveness, cancellationToken);
        }

        public async Task<HealthCheckResults> CheckReadiness(CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckServices(HealthCheckType.Readiness, cancellationToken);
        }

        public async Task<HealthCheckResults> CheckStartup(CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckServices(HealthCheckType.Startup, cancellationToken);
        }

        public async Task<HealthCheckResults> CheckStatus(CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckServices(HealthCheckType.Status, cancellationToken);
        }



        public T GetProbeService<T>()
        {
            return (T)_serviceProvider.GetRequiredService(typeof(T));
        }

        public async Task<HealthCheckResults> ExecuteCheckServices(HealthCheckType healthCheckType, CancellationToken cancellationToken)
        {
            IList<KeyValuePair<string, HealthCheckResult>> results = new List<KeyValuePair<string, HealthCheckResult>>();

            // Get registered services by key
            var services = _serviceProvider.GetKeyedServices<IHealthCheck>(healthCheckType);

            // Make it simple and iterate one at a time
            foreach (IHealthCheck service in services)
            {
                HealthCheckRegistration? registration = RegistrationRepository.Get(service.GetType(), healthCheckType);
                HealthCheckResult result = HealthCheckResult.Unhealthy();
                string name = registration?.Name ?? NO_REGISTRATION_NAME;

                try
                {
                    result = await service.CheckHealthAsync(cancellationToken);
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Health Check Custom Service: {0}", name);
                }
                    
                finally
                {
                    var kvp = new KeyValuePair<string, HealthCheckResult>(name, result);
                    results.Add(kvp);
                }
            }

            return new HealthCheckResults(DetermineOverallStatus(results), results);
        }

        private HealthStatus DetermineOverallStatus(IEnumerable<KeyValuePair<string, HealthCheckResult>> results)
        {
            HealthStatus ret = HealthStatus.Healthy;

            if (results.Any(kvp => kvp.Value.Status == HealthStatus.UnHealthy))
                ret = HealthStatus.UnHealthy;
            else if (results.Any(kvp => kvp.Value.Status == HealthStatus.Degraded))
                ret = HealthStatus.Degraded;

            return ret;
        }
    }
}
