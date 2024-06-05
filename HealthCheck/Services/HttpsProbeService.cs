using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using HealthCheck.Configuration;
using HealthCheck.Formatters;

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
    internal sealed class HttpsProbeService : HttpService, IHttpsProbeService
    {
        private const string CERTIFICATE_ISSUER = "TQL";
        private HttpProbeOptions? _httpProbeOptions;
        private ProbeLoggingOptions? _probeLoggingOptions;
        private X509Certificate2? _serverCertificate;

        public HttpsProbeService(ILogger<HttpProbeService> logger,
                                 IHealthCheckService healthCheckService)
            : base(logger, healthCheckService)
        {
        }

        public async Task Monitor(HttpProbeOptions probeOptions, ProbeLoggingOptions loggingOptions, CancellationToken cancellationToken)
        {
            _httpProbeOptions = probeOptions;
            _probeLoggingOptions = loggingOptions;

            // Yield so that caller can continue processing
            await Task.Yield();

            try
            {
                _serverCertificate = GetServerCertificate(StoreName.Root, CERTIFICATE_ISSUER) ??
                    throw new Exception($"Unable to acquire X509 Certificate issued by {CERTIFICATE_ISSUER}");

                await Execute(probeOptions.Endpoints, loggingOptions, cancellationToken);
            }

            // This would occur by cancellationToken if sub methods would use the ThrowIfCancelled
            catch (OperationCanceledException ex)
            {
                this.Logger.LogError(ex, "Https Probe Service shutting down by request");
            }

            // Any other exception
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Https Probe Service encountered an error and is shutting down");
            }


            await Task.CompletedTask;
        }


        protected override TcpListener CreateTcpListener()
        {
            ArgumentNullException.ThrowIfNull(_httpProbeOptions?.SslPort, "SslPort");
            return new TcpListener(IPAddress.Any, _httpProbeOptions.SslPort.Value);
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
            var stream = new SslStream(client.GetStream(), true);
            await stream.AuthenticateAsServerAsync(_serverCertificate, false, true);
            return stream;
        }


        private X509Certificate2? GetServerCertificate(StoreName storeName, string issuer)
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
                    var certsByIssuer = store.Certificates.Find(X509FindType.FindByIssuerName, issuer, true);

                    // Get cert where the expiration date is at least 1 day from current date
                    x509Certificate = store.Certificates
                                            .Find(X509FindType.FindByTimeValid, DateTime.Now.AddDays(1), true)
                                            .First();
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



        /// <summary>
        /// Get first valid certificate from this server from any cert store or location
        /// </summary>
        /// <returns></returns>
        private X509Certificate2? GetFirstValidServerCertificate()
        {
            X509Certificate2? x509Certificate = null;
            StoreLocation[] storeLocations = (StoreLocation[])Enum.GetValues(typeof(StoreLocation));
            StoreName[] storeNames = (StoreName[])Enum.GetValues(typeof(StoreName));

            foreach (StoreLocation storeLocation in storeLocations)
            {
                // If set, then break out of loop
                if (x509Certificate != null)
                    break;

                foreach (StoreName storeName in storeNames)
                {
                    X509Store store = new X509Store(storeName, storeLocation);

                    try
                    {
                        store.Open(OpenFlags.OpenExistingOnly);
                        if (store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true).Any())
                        {
                            // Get any valid x509 Certificate that can be used.
                            x509Certificate = store.Certificates
                                                   .Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                                                   .First();
                            break;
                        }
                    }

                    // Store cannot be openned
                    catch (CryptographicException)
                    { }

                    // Caller does not have the required permission
                    catch (SecurityException)
                    { }

                    catch (Exception)
                    { }
                }
            }

            return x509Certificate;
        }

    }
}

/*
 *
 *            // This is would occur during SSL Server side authentication
            catch (AuthenticationException)
            { }

            // This would occur by cancellationToken if sub methods would use the ThrowIfCancelled
            catch (OperationCanceledException)
            { }

            // Any other exception
            catch (Exception ex)
            {
                _logger.LogError(ex, "Http Probe Service encountered an error and is shutting down");
            }


            await Task.CompletedTask;
        }

*/