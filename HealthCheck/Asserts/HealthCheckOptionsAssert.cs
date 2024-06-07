using System;
using HealthCheck.Configuration;


namespace HealthCheck.Asserts
{
    internal static class HealthCheckOptionsAssert
    {
        public static void AssertNotValidInterval(int seconds)
        {
            const int MAX_SECONDS = 60;
            if (seconds < 0 || seconds > MAX_SECONDS)
                throw new Exception("Interval value must be between 1 and 30 seconds");
        }

        public static void AssertNotValidPort(int port)
        {
            if (port < 0 || port > 65535)
                throw new Exception("Port number must be between 0 and 65535");
        }

        public static void AssertNotSamePort(int? httpPort, int? sslPort)
        {
            if (httpPort == null || sslPort == null)
                return;

            if (httpPort.Value == sslPort.Value)
                throw new Exception("Http Probe Port and SslPort must not be the same");
        }


        public static void AssertNotSamePort(int? httpPort, TcpProbeOptions tcpProbe)
        {
            if (httpPort == null ||
                tcpProbe == null ||
                tcpProbe.Ports == null)
                return;

            if ((tcpProbe.Ports.Startup != null && httpPort.Value == tcpProbe.Ports.Startup.Value) ||
                (tcpProbe.Ports.Readiness != null && httpPort.Value == tcpProbe.Ports.Readiness.Value) ||
                (tcpProbe.Ports.Liveness != null && httpPort.Value == tcpProbe.Ports.Liveness.Value))
                throw new Exception("Http Probe port must not be same as any Tcp Probe ports");
        }

        public static void AssertNoProbesConfigured(HealthCheckOptions healthCheckOptions)
        {
            if (healthCheckOptions == null ||
                (healthCheckOptions.HttpProbe == null && healthCheckOptions.TcpProbe == null))
                throw new Exception("No probes have been configured");
        }
    }
}
