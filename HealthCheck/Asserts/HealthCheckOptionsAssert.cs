using System;


namespace HealthCheck.Asserts
{
    internal static class HealthCheckOptionsAssert
    {
        public static void AssertNotValidPort(int port, string? paramName)
        {
            if (port < 0 || port > 65535)
                throw new ArgumentException("Port number must be between 0 and 65535", paramName);
        }
    }
}
