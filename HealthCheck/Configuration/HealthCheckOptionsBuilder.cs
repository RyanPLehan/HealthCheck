using System;
using Microsoft.Extensions.Configuration;
using HealthCheck.Asserts;


namespace HealthCheck.Configuration
{
    public class HealthCheckOptionsBuilder
    {
        public const string CONFIGURATION_SECTION = "HealthCheck";
        private StatusListenerOptions _statusOptions = null;
        private ProbeOptions _startupOptions = null;
        private ProbeOptions _readinessOptions = null;
        private ProbeOptions _livenessOptions = null;

        /// <summary>
        /// Load options from configuration file using default section name
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static HealthCheckOptions Build(IConfiguration configuration)
            => Build(configuration, CONFIGURATION_SECTION);


        /// <summary>
        /// Load options from configuration file using given section name
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static HealthCheckOptions Build(IConfiguration configuration, string sectionName)
        {
            HealthCheckOptions options = new HealthCheckOptions();
            configuration.GetSection(sectionName).Bind(options);
            return options;
        }

        /// <summary>
        /// Build Options
        /// </summary>
        /// <returns></returns>
        public HealthCheckOptions Build()
        {
            return new HealthCheckOptions()
            {
                Status = _statusOptions,
                Startup = _startupOptions,
                Readiness = _readinessOptions,
                Liveness = _livenessOptions,
            };
        }

        /// <summary>
        /// Add Status listener information
        /// </summary>
        /// <param name="port"></param>
        /// <param name="endpoint"></param>
        /// <remarks>
        /// Health Check Status will always be via HTTP
        /// </remarks>
        public void AddStatus(int port, string endpoint)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));
            ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));
            
            _statusOptions = new StatusListenerOptions()
            {
                Port = port,
                EndPoint = endpoint,
            };
        }

        /// <summary>
        /// Add HTTP Startup Probe listener information
        /// </summary>
        /// <param name="port"></param>
        /// <param name="endpoint"></param>
        public void AddHttpStartup(int port, string endpoint)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));
            ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

            _startupOptions =new ProbeOptions()
            {
                HealthCheckProbeType = HealthCheckProbeType.Http,
                Port = port,
                EndPoint = endpoint,
            };
        }

        /// <summary>
        /// Add HTTP Readiness Probe listener information
        /// </summary>
        /// <param name="port"></param>
        /// <param name="endpoint"></param>
        public void AddHttpReadiness(int port, string endpoint)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));
            ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

            _readinessOptions =new ProbeOptions()
            {
                HealthCheckProbeType = HealthCheckProbeType.Http,
                Port = port,
                EndPoint = endpoint,
            };
        }

        /// <summary>
        /// Add HTTP Liveness Probe listener information
        /// </summary>
        /// <param name="port"></param>
        /// <param name="endpoint"></param>
        public void AddHttpLiveness(int port, string endpoint)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));
            ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

            _livenessOptions =new ProbeOptions()
            {
                HealthCheckProbeType = HealthCheckProbeType.Http,
                Port = port,
                EndPoint = endpoint,
            };
        }

        /// <summary>
        /// Add TCP Startup Probe listener information
        /// </summary>
        /// <param name="port"></param>
        public void AddTcpStartup(int port)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));

            _startupOptions =new ProbeOptions()
            {
                HealthCheckProbeType = HealthCheckProbeType.Tcp,
                Port = port,
                EndPoint = null,
            };
        }

        /// <summary>
        /// Add TCP Readiness Probe listener information
        /// </summary>
        /// <param name="port"></param>
        public void AddTcpReadiness(int port)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));

            _readinessOptions =new ProbeOptions()
            {
                HealthCheckProbeType = HealthCheckProbeType.Tcp,
                Port = port,
                EndPoint = null,
            };
        }

        /// <summary>
        /// Add TCP Liveness Probe listener information
        /// </summary>
        /// <param name="port"></param>
        public void AddTcpLiveness(int port)
        {
            HealthCheckOptionsAssert.AssertNotValidPort(port, nameof(port));

            _livenessOptions =new ProbeOptions()
            {
                HealthCheckProbeType = HealthCheckProbeType.Tcp,
                Port = port,
                EndPoint = null,
            };
        }
    }
}
