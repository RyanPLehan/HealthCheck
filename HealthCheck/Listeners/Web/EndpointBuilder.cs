using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCheck.Listeners.Web
{
    internal class EndpointBuilder : IEndpointBuilder
    {
        private readonly IServiceCollection _services;

        /// <summary>
        /// Build HTTP Endpoints
        /// </summary>
        /// <param name="services"></param>
        /// <remarks>
        /// This will be used for both HTTP and HTTPS Listeners
        /// <see cref="https://github.com/dotnet/AspNetCore/blob/main/src/Middleware/HealthChecks/src/Builder/HealthCheckEndpointRouteBuilderExtensions.cs"/>
        /// </remarks>
        public EndpointBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            _services = services;
        }

        public IServiceCollection Services => _services;

        public IEndpointBuilder MapHealthChecks([StringSyntax("Route")] string pattern)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

            // TODO: Create class that adds endpoint to Enumerable Singleton service
            return this;
        }
    }
}
