using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthCheck.Registration
{
    public static class ServiceCollectionExtension
    {
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
        {
            return new HealthChecksBuilder(services);
        }

        public static IHttpEndpointBuilder MapHttpEndpoints(this IServiceCollection services)
        {
            return new HttpEndpointBuilder(services);
        }

        /*
        /// <summary>
        /// Add Health Checks
        /// </summary>
        /// <param name="services"></param>
        /// <param name="namedConfigurationSection">
        /// <see cref="https://learn.microsoft.com/en-us/dotnet/core/extensions/options-library-authors#iconfiguration-parameter"/> 
        /// </param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services, IConfigurationSection namedConfigurationSection)
        {
            // Explicity Set Configuration from named configutation section passed in
            // Any one of these will work
            //services.AddOptions<HealthCheckOptions>().BindConfiguration(HealthCheckOptionsBuilder.CONFIGURATION_SECTION);     
            //services.AddOptions().Configure<HealthCheckOptions>(configuration.GetSection(HealthCheckOptionsBuilder.CONFIGURATION_SECTION))
            //services.Configure<HealthCheckOptions>(configuration.GetSection(HealthCheckOptionsBuilder.CONFIGURATION_SECTION));
            //services.AddOptions().Configure<HealthCheckOptions>(namedConfigurationSection);            
            //services.Configure<HealthCheckOptions>(namedConfigurationSection);

            bool isValid = (namedConfigurationSection.GetChildren().Count() > 0);

            services.AddOptions<HealthCheckOptions>()
                    .Bind(namedConfigurationSection)
                    .Validate((_) => isValid)
                    .ValidateOnStart();

            return AddHealthChecks(services);
        }
        */
    }
}
