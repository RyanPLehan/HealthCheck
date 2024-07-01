﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HealthCheck.Configuration;
using HealthCheck.Listeners.Web;

namespace HealthCheck.Listeners.Web.Http
{
    /// <summary>
    /// This will respond to HTTP probes using the given port by issuing HTTP 200 or HTTP 503 status codes
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
    internal sealed class HttpListener : ListenerBase
    {
        private readonly HttpListenerOptions _ListenerOptions;

        public HttpListener(ILogger<HttpListener> logger,
                           IHealthCheckServiceProvider healthCheckService,
                           IOptions<ListenerLogOptions> probeLogOptions,
                           IOptions<HttpListenerOptions> ListenerOptions)
            : base(logger, healthCheckService, probeLogOptions)
        {
            ArgumentNullException.ThrowIfNull(ListenerOptions?.Value, nameof(ListenerOptions));
            _ListenerOptions = ListenerOptions.Value;
        }


        /// <summary>
        /// Create TCP Listener
        /// </summary>
        /// <returns></returns>
        protected override TcpListener CreateTcpListener()
        {
            Asserts.Argument.AssertNotValidPort(_ListenerOptions.Port);
            return new TcpListener(IPAddress.Any, _ListenerOptions.Port);
        }
    }
}