using HealthCheck.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthCheck
{
    internal class HealthCheckServer : BackgroundService
    {
        private readonly ILogger<HealthCheckServer> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly HealthCheckOptions _options;


        public HealthCheckServer(ILogger<HealthCheckServer> logger,
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


        public HealthCheckServer(ILogger<HealthCheckServer> logger,
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
                _logger.LogInformation("Starting: Health Check Monitor");
                task = base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health Check Monitor failed to start");
                task = Task.FromException(ex);
            }

            return task;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping: Health Check Monitor");
            return base.StopAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Health Check Monitor running at: {time}", DateTimeOffset.Now);

            IList<Task> tasks = new List<Task>();
            tasks.Add(StartHttpMonitor(cancellationToken));
            tasks.Add(StartHttpsMonitor(cancellationToken));
            tasks.Add(StartTcpMonitor(cancellationToken));

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Stop background service when all tasks are complete
            await this.StopAsync(cancellationToken);
        }

        private void ValidateOptions()
        {
            Asserts.HealthCheckOptionsAssert.AssertNoProbesConfigured(this._options);
            Asserts.HealthCheckOptionsAssert.AssertNotSamePort(_options.HttpProbe?.Port, _options.HttpProbe?.SslPort);
            Asserts.HealthCheckOptionsAssert.AssertNotSamePort(_options.HttpProbe?.Port, _options.TcpProbe);
            Asserts.HealthCheckOptionsAssert.AssertNotSamePort(_options.HttpProbe?.SslPort, _options.TcpProbe);

            if (_options.HttpProbe?.Port != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.HttpProbe.Port.Value);

            if (_options.HttpProbe?.SslPort != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.HttpProbe.SslPort.Value);

            if (_options.TcpProbe != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidInterval(_options.TcpProbe.CheckRetryIntervalInSeconds);
            
            if (_options.TcpProbe?.Ports.Startup != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.TcpProbe.Ports.Startup.Value);

            if (_options.TcpProbe?.Ports.Readiness != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.TcpProbe.Ports.Readiness.Value);

            if (_options.TcpProbe?.Ports.Liveness != null)
                Asserts.HealthCheckOptionsAssert.AssertNotValidPort(_options.TcpProbe.Ports.Liveness.Value);
        }


        private Task StartHttpMonitor(CancellationToken cancellationToken)
        {
            Task task = Task.CompletedTask;

            if (_options.HttpProbe?.Port != null)
            {
                IHttpMonitor service = _healthCheckService.GetMonitorService<IHttpMonitor>();
                task = service.Monitor(_options.HttpProbe, _options.Logging, cancellationToken);
            }

            return task;
        }

        private Task StartHttpsMonitor(CancellationToken cancellationToken)
        {
            Task task = Task.CompletedTask;

            if (_options.HttpProbe?.SslPort != null)
            {
                IHttpsMonitor service = _healthCheckService.GetMonitorService<IHttpsMonitor>();
                task = service.Monitor(_options.HttpProbe, _options.Logging, cancellationToken);
            }

            return task;
        }

        private Task StartTcpMonitor(CancellationToken cancellationToken)
        {
            Task task = Task.CompletedTask;

            if (_options.TcpProbe != null)
            {
                ITcpMonitor service = _healthCheckService.GetMonitorService<ITcpMonitor>();
                task = service.Monitor(_options.TcpProbe, _options.Logging, cancellationToken);
            }

            return task;
        }
    }
}
