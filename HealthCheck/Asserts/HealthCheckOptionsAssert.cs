using HealthCheck.Configuration;
using System;


namespace HealthCheck.Asserts
{
    internal static class HealthCheckOptionsAssert
    {
        public static void AssertNotValidInterval(int seconds)
        {
            const int MAX_SECONDS = 30;
            if (seconds < 0 || seconds > MAX_SECONDS)
                throw new Exception("Interval value must be between 1 and 30 seconds");
        }

        public static void AssertNotValidPort(int port)
        {
            if (port < 0 || port > 65535)
                throw new Exception("Port number must be between 0 and 65535");
        }

        public static void AssertNotSamePort(HttpProbeOptions httpProbe, TcpProbeOptions tcpProbe)
        {
            if (httpProbe == null ||
                tcpProbe == null ||
                tcpProbe.Ports == null)
                return;

            if ((tcpProbe.Ports.Startup != null && httpProbe.Port == tcpProbe.Ports.Startup) ||
                (tcpProbe.Ports.Readiness != null && httpProbe.Port == tcpProbe.Ports.Readiness) ||
                (tcpProbe.Ports.Liveness != null && httpProbe.Port == tcpProbe.Ports.Liveness))
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
