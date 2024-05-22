using HealthCheck.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthCheck
{
    public class HealthCheckWorker : BackgroundService
    {
        private readonly ILogger<HealthCheckWorker> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly HealthCheckOptions _options;


        internal HealthCheckWorker(ILogger<HealthCheckWorker> logger,
                                   IHealthCheckService healthCheckService,
                                   IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            _logger = logger;
            _healthCheckService = healthCheckService;
            _options = HealthCheckOptionsBuilder.Build(configuration) ??
                throw new Exception($"Unable to load settings from section '{HealthCheckOptionsBuilder.CONFIGURATION_SECTION}'");
        }


        internal HealthCheckWorker(ILogger<HealthCheckWorker> logger,
                                   IHealthCheckService healthCheckService,
                                   IOptions<HealthCheckOptions> options)
            : this(logger, healthCheckService, options.Value)
        { }

        internal HealthCheckWorker(ILogger<HealthCheckWorker> logger,
                                   IHealthCheckService healthCheckService,
                                   HealthCheckOptions options )
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            _logger = logger;
            _healthCheckService = healthCheckService;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting: Health Check Worker");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping: Health Check Worker");

            return base.StopAsync(cancellationToken);
        }
    }
}
