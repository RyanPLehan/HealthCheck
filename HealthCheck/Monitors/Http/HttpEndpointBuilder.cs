using System;
using System.Diagnostics.CodeAnalysis;
using HealthCheck;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCheck.Registration
{
    internal class HttpEndpointBuilder : IHttpEndpointBuilder
    {
        private readonly IServiceCollection _services;

        /// <summary>
        /// Build HTTP Endpoints
        /// </summary>
        /// <param name="services"></param>
        /// <remarks>
        /// This will be used for both HTTP and HTTPS monitors
        /// <see cref="https://github.com/dotnet/AspNetCore/blob/main/src/Middleware/HealthChecks/src/Builder/HealthCheckEndpointRouteBuilderExtensions.cs"/>
        /// </remarks>
        public HttpEndpointBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            _services = services;
        }

        public IServiceCollection Services => _services;

        public IHttpEndpointBuilder MapHealthChecks([StringSyntax("Route")] string pattern)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

            // TODO: Create class that adds endpoint to Enumerable Singleton service
            return this;
        }
    }
}
