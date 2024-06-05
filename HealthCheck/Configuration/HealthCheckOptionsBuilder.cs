using System;
using Microsoft.Extensions.Configuration;
using HealthCheck.Asserts;


namespace HealthCheck.Configuration
{
    public class HealthCheckOptionsBuilder
    {
        public const string CONFIGURATION_SECTION = "HealthCheck";
        private ProbeLoggingOptions? _loggingOptions = null;
        private HttpProbeOptions? _httpOptions = null;
        private TcpProbeOptions? _tcpOptions = null;


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
                Logging = _loggingOptions,
                HttpProbe = _httpOptions,
                TcpProbe = _tcpOptions,
            };
        }

        /// <summary>
        /// Add Logging
        /// </summary>
        /// <param name="logProbe"></param>
        /// <param name="logStatusWhenHealthy"></param>
        /// <param name="logStatusWhenNotHealthy"></param>
        public void AddLogging(bool logProbe = true,
                               bool logStatusWhenHealthy = false,
                               bool logStatusWhenNotHealthy = true)
        {
            _loggingOptions = new ProbeLoggingOptions()
            {
                LogProbe = logProbe,
                LogStatusWhenHealthy = logStatusWhenHealthy,
                LogStatusWhenNotHealthy = logStatusWhenNotHealthy
            };
        }


        /// <summary>
        /// Add Http Probe
        /// </summary>
        /// <param name="port"></param>
        /// <param name="statusEndpoint"></param>
        /// <param name="startupEndpoint"></param>
        /// <param name="readinessEndpoint"></param>
        /// <param name="livenessEndpoint"></param>
        /// <remarks>
        /// If there is a specific probe that is not warranted, just set to null
        /// A blank endpoint or "/" will be the root
        /// </remarks>
        public void AddHttpProbe(int? port = 80,
                                 int? sslPort = 443,
                                 string statusEndpoint = "health/status",
                                 string startupEndpoint = "health/startup",
                                 string readinessEndpoint = "health/readiness",
                                 string livenessEndpoint = "health/liveness")
        {
            _httpOptions = new HttpProbeOptions()
            {
                Port = port,
                SslPort = sslPort,
                Endpoints = new EndpointAssignment()
                {
                    Status = statusEndpoint,
                    Startup = startupEndpoint,
                    Readiness = readinessEndpoint,
                    Liveness = livenessEndpoint,
                },
            };
        }

        /// <summary>
        /// Add Tcp Probe
        /// </summary>
        /// <param name="startupPort"></param>
        /// <param name="readinessPort"></param>
        /// <param name="livenessPort"></param>
        /// <remarks>
        /// If there is a specific probe that is not warranted, set the port to null
        /// Tcp ports must not match Http Port, if http probe is being used 
        /// </remarks>
        public void AddTcpProbe(int? startupPort = 8081,
                                int? readinessPort = 8081,
                                int? livenessPort = 8081)
        {
            _tcpOptions = new TcpProbeOptions()
            {
                Ports = new PortAssignment()
                {
                    Startup = startupPort,
                    Readiness = readinessPort,
                    Liveness = livenessPort,
                },
            };
        }
    }
}
