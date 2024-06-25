using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HealthCheck.Configuration;


namespace HealthCheck.Services
{
    /// <summary>
    /// This will respond to HTTPS probes
    /// </summary>
    /// <remarks>
    /// This is a bare minimum HTTP Server that will specifically look for endpoints.
    /// If a matching endpoint is found, then it will execute the Health Check services for the associated health check type
    /// This is a typical Request/Response in that
    /// a. A HTTP GET request is made to a specific end point
    /// b. The endpoint is scanned to determine if it is a Health Check endpoint
    /// c. Execution of one or more Health Checks for that endpoint is executed and compiled into a single object
    /// d. If the endpoint is registered to Health Check Status Request, then the response will send a Json object of all Health Check Service Results
    /// e. If the endpoint is registered to Health Check Startup, Readiness or Liveness then one of the following responses will be returned
    ///     1.  HTTP 200 OK if ALL Health Check Services returns Healthy
    ///     2.  HTTP 503 Service Unavailable if just ONE Health Check Service returns a Degraded or Unhealthy result
    /// f. Other types of responses are as follows
    ///     1.  If the request is not a HTTP GET method, then a 405 Method Not Allowed is returned
    ///     2.  If an endpoint is not matched, then a 404 Not Found is returned
    /// </remarks>
    internal sealed class HttpsMonitor : HttpMonitorBase
    {
        private readonly HttpsMonitorOptions _monitorOptions;
        private X509Certificate2? _serverCertificate;

        public HttpsMonitor(ILogger<HttpsMonitor> logger,
                            IHealthCheckService healthCheckService,
                            IOptions<ProbeLogOptions> probeLogOptions,
                            IOptions<HttpsMonitorOptions> monitorOptions)
            : base(logger, healthCheckService, probeLogOptions)
        {
            _monitorOptions = monitorOptions?.Value ??
                throw new ArgumentNullException(nameof(monitorOptions));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!String.IsNullOrWhiteSpace(_monitorOptions.UseCertificateByIssuerName))
                _serverCertificate = GetServerCertificateByIssuer(StoreName.Root, _monitorOptions.UseCertificateByIssuerName);
            else if (!String.IsNullOrWhiteSpace(_monitorOptions.UseCertificateBySubjectName))
                _serverCertificate = GetServerCertificateBySubject(StoreName.Root, _monitorOptions.UseCertificateBySubjectName);
            else
                _serverCertificate = GetServerCertificate(StoreName.Root);


            if (_serverCertificate != null)
            {
                await base.ExecuteAsync(cancellationToken);
            }
            else
            {
                Logger.LogError("Unable to acquire X509 Certificate");
                await StopAsync(cancellationToken);
            }
        }


        /// <summary>
        /// Create TCP Listener
        /// </summary>
        /// <returns></returns>
        protected override TcpListener CreateTcpListener()
        {
            Asserts.Argument.AssertNotValidPort(_monitorOptions.Port);
            return new TcpListener(IPAddress.Any, _monitorOptions.Port);
        }


        /// <summary>
        /// Create SSL Stream
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <remarks>
        /// Authenticate the server, but don't require the client to authenticate
        /// Code is taken from https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?view=net-8.0&redirectedfrom=MSDN
        /// </remarks>
        protected override async Task<Stream> GetClientStream(TcpClient client)
        {
            var stream = new SslStream(client.GetStream(), false);
            await stream.AuthenticateAsServerAsync(_serverCertificate, false, true);
            return stream;
        }


        /// <summary>
        /// Get first valid certificate from this server from any cert store or location
        /// </summary>
        /// <returns></returns>
        private X509Certificate2? GetServerCertificate()
        {
            X509Certificate2? x509Certificate = null;
            StoreName[] storeNames = (StoreName[])Enum.GetValues(typeof(StoreName));

            foreach (StoreName storeName in storeNames)
            {
                x509Certificate = GetServerCertificate(storeName);

                // If set, then break out of loop
                if (x509Certificate != null)
                    break;
            }

            return x509Certificate;
        }


        private X509Certificate2? GetServerCertificateByIssuer(StoreName storeName, string issuerName)
        {
            X509Certificate2? x509Certificate = null;
            StoreLocation[] storeLocations = (StoreLocation[])Enum.GetValues(typeof(StoreLocation));

            // Iterate through store locations (ie current user and local machine)
            foreach (StoreLocation storeLocation in storeLocations)
            {
                X509Store store = new X509Store(storeName, storeLocation);

                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    var certsByIssuer = store.Certificates
                                             .Find(X509FindType.FindByIssuerName, issuerName, true);

                    x509Certificate = GetServerCertificate(certsByIssuer);
                    if (x509Certificate != null)
                        break;
                }

                // Certificate Store cannot be openned
                catch (CryptographicException)
                { }

                // Caller does not have the required permission for Certificate
                catch (SecurityException)
                { }
            }

            return x509Certificate;
        }


        private X509Certificate2? GetServerCertificateBySubject(StoreName storeName, string subjectName)
        {
            X509Certificate2? x509Certificate = null;
            StoreLocation[] storeLocations = (StoreLocation[])Enum.GetValues(typeof(StoreLocation));

            // Iterate through store locations (ie current user and local machine)
            foreach (StoreLocation storeLocation in storeLocations)
            {
                X509Store store = new X509Store(storeName, storeLocation);

                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    var certsByIssuer = store.Certificates
                                             .Find(X509FindType.FindBySubjectName, subjectName, true);

                    x509Certificate = GetServerCertificate(certsByIssuer);
                    if (x509Certificate != null)
                        break;
                }

                // Certificate Store cannot be openned
                catch (CryptographicException)
                { }

                // Caller does not have the required permission for Certificate
                catch (SecurityException)
                { }
            }

            return x509Certificate;
        }


        private X509Certificate2? GetServerCertificate(StoreName storeName)
        {
            X509Certificate2? x509Certificate = null;
            StoreLocation[] storeLocations = (StoreLocation[])Enum.GetValues(typeof(StoreLocation));

            // Iterate through store locations (ie current user and local machine)
            foreach (StoreLocation storeLocation in storeLocations)
            {
                X509Store store = new X509Store(storeName, storeLocation);

                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    x509Certificate = GetServerCertificate(store.Certificates);
                    if (x509Certificate != null)
                        break;
                }

                // Certificate Store cannot be openned
                catch (CryptographicException)
                { }

                // Caller does not have the required permission for Certificate
                catch (SecurityException)
                { }
            }

            return x509Certificate;
        }

        private X509Certificate2? GetServerCertificate(X509Certificate2Collection certificates)
        {
            return certificates.OfType<X509Certificate2>()
                                          .Where(IsCertificateAllowedForServerAuthentication)
                                          .Where(cert => cert.HasPrivateKey)
                                          .OrderByDescending(cert => cert.NotAfter)
                                          .FirstOrDefault();
        }

        private bool IsCertificateAllowedForServerAuthentication(X509Certificate2 certificate)
        {
            // See http://oid-info.com/get/1.3.6.1.5.5.7.3.1
            // Indicates that a certificate can be used as a SSL server certificate
            const string SERVER_AUTHENTICATION_OID = "1.3.6.1.5.5.7.3.1";

            var hasEkuExtension = false;

            foreach (var extension in certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>())
            {
                hasEkuExtension = true;
                foreach (var oid in extension.EnhancedKeyUsages)
                {
                    if (string.Equals(oid.Value, SERVER_AUTHENTICATION_OID, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return !hasEkuExtension;

        }
    }
}