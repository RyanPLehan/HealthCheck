﻿using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthCheck.DefaultChecks;

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

            // If dev adds their own check status, remove default status check
            RemoveService<LivenessCheck>(HealthCheckType.Liveness);
            RemoveRegistration<LivenessCheck>(HealthCheckType.Liveness);

            CreateRegistration(typeof(TService), HealthCheckType.Liveness, name);
            _services.TryAddKeyedSingleton<IHealthCheck, TService>(HealthCheckType.Liveness);
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

            // If dev adds their own check status, remove default status check
            RemoveService<LivenessCheck>(HealthCheckType.Liveness);
            RemoveRegistration<LivenessCheck>(HealthCheckType.Liveness);

            CreateRegistration(instance.GetType(), HealthCheckType.Liveness, name);
            _services.TryAddKeyedSingleton<IHealthCheck>(HealthCheckType.Liveness, instance);
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

            // If dev adds their own check status, remove default status check
            RemoveService<ReadinessCheck>(HealthCheckType.Readiness);
            RemoveRegistration<ReadinessCheck>(HealthCheckType.Readiness);

            CreateRegistration(typeof(TService), HealthCheckType.Readiness, name);
            _services.TryAddKeyedSingleton<IHealthCheck, TService>(HealthCheckType.Readiness);
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

            // If dev adds their own check status, remove default status check
            RemoveService<ReadinessCheck>(HealthCheckType.Readiness);
            RemoveRegistration<ReadinessCheck>(HealthCheckType.Readiness);

            CreateRegistration(instance.GetType(), HealthCheckType.Readiness, name);
            _services.TryAddKeyedSingleton<IHealthCheck>(HealthCheckType.Readiness, instance);
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

            // If dev adds their own check status, remove default status check
            RemoveService<StartupCheck>(HealthCheckType.Startup);
            RemoveRegistration<StartupCheck>(HealthCheckType.Startup);

            CreateRegistration(typeof(TService), HealthCheckType.Startup, name);
            _services.TryAddKeyedSingleton<IHealthCheck, TService>(HealthCheckType.Startup);
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

            // If dev adds their own check status, remove default status check
            RemoveService<StartupCheck>(HealthCheckType.Startup);
            RemoveRegistration<StartupCheck>(HealthCheckType.Startup);

            CreateRegistration(instance.GetType(), HealthCheckType.Startup, name);
            _services.TryAddKeyedSingleton<IHealthCheck>(HealthCheckType.Startup, instance);
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

            // If dev adds their own check status, remove default status check
            // Only because the default status check returns Unhealthy
            RemoveService<StatusCheck>(HealthCheckType.Status);
            RemoveRegistration<StatusCheck>(HealthCheckType.Status);

            CreateRegistration(typeof(TService), HealthCheckType.Status, name);
            _services.TryAddKeyedSingleton<IHealthCheck, TService>(HealthCheckType.Status);
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

            // If dev adds their own check status, remove default status check
            // Only because the default status check returns Unhealthy
            RemoveService<StatusCheck>(HealthCheckType.Status);
            RemoveRegistration<StatusCheck>(HealthCheckType.Status);

            CreateRegistration(instance.GetType(), HealthCheckType.Status, name);
            _services.TryAddKeyedSingleton<IHealthCheck>(HealthCheckType.Status, instance);
            return this;
        }
        #endregion

        private HealthCheckRegistration CreateRegistration<T>(HealthCheckType healthCheckType, string name)
            => CreateRegistration(typeof(T), healthCheckType, name);


        private HealthCheckRegistration CreateRegistration(Type type, HealthCheckType healthCheckType, string name)
        {
            HealthCheckRegistration registration = new HealthCheckRegistration()
            {
                Type = type,
                HealthCheckType = healthCheckType,
                Name = name,
            };

            RegistrationRepository.Add(registration);

            return registration;
        }

        private void RemoveRegistration<T>(HealthCheckType healthCheckType)
            => RemoveRegistration(typeof(T), healthCheckType);

        private void RemoveRegistration(Type type, HealthCheckType healthCheckType)
        {
            RegistrationRepository.Remove(type, healthCheckType);
        }

        private void RemoveService<T>(HealthCheckType healthCheckType)
            => RemoveService(typeof(T), healthCheckType);

        private void RemoveService(Type type, HealthCheckType healthCheckType)
        {
            if (_services.IsReadOnly)
                throw new ReadOnlyException("ServiceCollection is read only");

            // This works for non-keyed services
            //var serviceDescriptor = _services.Where(x => x.ServiceType == type)
            //                                 .FirstOrDefault();

            // This is for keyed services
            var serviceDescriptor = _services.Where(x => x.ServiceType == typeof(IHealthCheck) &&
                                                         (HealthCheckType)x.ServiceKey == healthCheckType &&
                                                         x.KeyedImplementationType == type) 
                                             .FirstOrDefault();

            if (serviceDescriptor != null) 
                _services.Remove(serviceDescriptor);
        }
    }
}
