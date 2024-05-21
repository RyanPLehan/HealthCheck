using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace HealthCheck
{
    /// <summary>
    /// This will execute the custom health check services
    /// </summary>
    internal class HealthCheckService : IHealthCheckService
    {
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


        public async Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckLiveness(CancellationToken cancellationToken = default)
        {
            return await ExecuteServices(HealthCheckType.Liveness, cancellationToken);
        }

        public async Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckReadiness(CancellationToken cancellationToken = default)
        {
            return await ExecuteServices(HealthCheckType.Readiness, cancellationToken);
        }

        public async Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckStartup(CancellationToken cancellationToken = default)
        {
            return await ExecuteServices(HealthCheckType.Startup, cancellationToken);
        }

        public async Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> CheckStatus(CancellationToken cancellationToken = default)
        {
            return await ExecuteServices(HealthCheckType.Status, cancellationToken);
        }

        private async Task<IEnumerable<KeyValuePair<string, HealthCheckResult>>> ExecuteServices(HealthCheckType healthCheckType,
                                                                                                 CancellationToken cancellationToken)
        {
            IList<KeyValuePair<string, HealthCheckResult>> results = new List<KeyValuePair<string, HealthCheckResult>>();

            // Get registered services by key
            var services = _serviceProvider.GetKeyedServices<IHealthCheck>(healthCheckType);

            // Make it simple and iterate one at a time
            foreach (IHealthCheck service in services)
            {
                string name = ServiceNameRepo.Get(service.GetType());
                HealthCheckResult result = HealthCheckResult.Unhealthy();

                try
                {
                    result = await service.CheckHealthAsync(cancellationToken);
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Health Check Custom Service: {0}", service.GetType().FullName);
                }
                    
                finally
                {
                    var kvp = new KeyValuePair<string, HealthCheckResult>(name, result);
                    results.Add(kvp);
                }
            }

            return results;
        }
    }
}
