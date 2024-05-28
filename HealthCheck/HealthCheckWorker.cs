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


        public HealthCheckWorker(ILogger<HealthCheckWorker> logger,
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


        public HealthCheckWorker(ILogger<HealthCheckWorker> logger,
                                 IHealthCheckService healthCheckService,
                                 HealthCheckOptions options)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(healthCheckService, nameof(healthCheckService));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            _logger = logger;
            _healthCheckService = healthCheckService;
            _options = options;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Task task;
            try
            {
                ValidateOptions();
                _logger.LogInformation("Starting: Health Check Worker");
                task = base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health Check Worker failed to start");
                task = Task.FromException(ex);
            }

            return task;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping: Health Check Worker");
            return base.StopAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Check Worker running at: {time}", DateTimeOffset.Now);

            IList<Task> tasks = new List<Task>();
            tasks.Add(StartHttpProbe(stoppingToken));

            await Task.WhenAll(tasks);
            
            //_logger.LogInformation("Running health checks");
            //IEnumerable<KeyValuePair<string, HealthCheckResult>> results;
            //results = await _healthCheckService.CheckStartup(stoppingToken);
            //LogResults(results);
            //results = await _healthCheckService.CheckReadiness(stoppingToken);
            //LogResults(results);
            //results = await _healthCheckService.CheckLiveness(stoppingToken);
            //LogResults(results);
            //results = await _healthCheckService.CheckStatus(stoppingToken);
            //LogResults(results);




            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Health Check Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}
        }


        private void LogResults(IEnumerable<KeyValuePair<string, HealthCheckResult>> results)
        {
            foreach (KeyValuePair<string, HealthCheckResult> result in results)
                _logger.LogInformation("Check {0} - Result {1}", result.Key, result.Value.Status.ToString());
        }



        private void ValidateOptions()
        {
            Asserts.HealthCheckOptionsAssert.AssertNoProbesConfigured(this._options);
            Asserts.HealthCheckOptionsAssert.AssertNotSamePort(_options.HttpProbe, _options.TcpProbe);

            if (_options.HttpProbe != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.HttpProbe.Port);

            if (_options.TcpProbe?.Ports.Startup != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.TcpProbe.Ports.Startup.Value);

            if (_options.TcpProbe?.Ports.Readiness != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.TcpProbe.Ports.Readiness.Value);

            if (_options.TcpProbe?.Ports.Liveness != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.TcpProbe.Ports.Liveness.Value);
        }


        private Task StartHttpProbe(CancellationToken cancellationToken)
        {
            IHttpProbeService service = _healthCheckService.GetProbeService<IHttpProbeService>();
            return service.Monitor(_options.HttpProbe, _options.Logging, cancellationToken);
        }
    }
}
