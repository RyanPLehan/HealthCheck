using System;

namespace HealthCheck.Listeners.Web.Https
{
    public class HttpsListenerOptions
    {
        public int Port { get; set; } = 443;
        public string UseCertificateByIssuerName { get; set; }
        public string UseCertificateBySubjectName { get; set; }
    }
}
