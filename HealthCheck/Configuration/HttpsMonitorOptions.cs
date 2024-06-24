using HealthCheck;
using System;

namespace HealthCheck.Configuration
{
    public class HttpsMonitorOptions
    {
        public int Port { get; set; } = 443;
        public string CertificateIssuer { get; set; }
    }
}
