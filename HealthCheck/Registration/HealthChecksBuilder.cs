using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.Registration
{
    internal sealed class HealthChecksBuilder : IHealthChecksBuilder
    {
        private readonly IServiceCollection _services;
        public HealthChecksBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            _services = services;
        }

        public IServiceCollection Services => _services;

        #region Check Liveness
        /// <summary>
        /// Add Health Check for Liveness Probe
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckLiveness<TService>(string name)
            where TService : class, IHealthCheck
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(typeof(TService), name);
            _services.TryAddKeyedSingleton<TService>(HealthCheckType.Liveness);
            return this;
        }


        /// <summary>
        /// Add Health Check for Liveness Probe
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckLiveness(IHealthCheck instance, string name)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(instance.GetType(), name);
            _services.TryAddKeyedSingleton(HealthCheckType.Liveness, instance);
            return this;
        }
        #endregion


        #region Check Readiness
        /// <summary>
        /// Add Health Check for Readiness Probe
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckReadiness<TService>(string name)
            where TService : class, IHealthCheck
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(typeof(TService), name);
            _services.TryAddKeyedSingleton<TService>(HealthCheckType.Readiness);
            return this;
        }


        /// <summary>
        /// Add Health Check for Readiness Probe
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckReadiness(IHealthCheck instance, string name)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(instance.GetType(), name);
            _services.TryAddKeyedSingleton(HealthCheckType.Readiness, instance);
            return this;
        }
        #endregion


        #region Check Startup
        /// <summary>
        /// Add Health Check for Startup Probe
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckStartup<TService>(string name)
            where TService : class, IHealthCheck
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(typeof(TService), name);
            _services.TryAddKeyedSingleton<TService>(HealthCheckType.Startup);
            return this;
        }


        /// <summary>
        /// Add Health Check for Startup Probe
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckStartup(IHealthCheck instance, string name)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(instance.GetType(), name);
            _services.TryAddKeyedSingleton(HealthCheckType.Startup, instance);
            return this;
        }
        #endregion


        #region Check Status
        /// <summary>
        /// Add Health Check for Status Probe
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckStatus<TService>(string name)
            where TService : class, IHealthCheck
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(typeof(TService), name);
            _services.TryAddKeyedSingleton<TService>(HealthCheckType.Status);
            return this;
        }


        /// <summary>
        /// Add Health Check for Status Probe
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheckStatus(IHealthCheck instance, string name)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            ServiceNameRepo.Add(instance.GetType(), name);
            _services.TryAddKeyedSingleton(HealthCheckType.Status, instance);
            return this;
        }
        #endregion
    }
}
