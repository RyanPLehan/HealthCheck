using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthCheck.DefaultChecks;

namespace HealthCheck.Registration
{
    /// <summary>
    /// Mimic closely to MS HealthCheckBuilder
    /// <see cref="https://github.com/dotnet/aspnetcore/blob/main/src/HealthChecks/HealthChecks/src/DependencyInjection/HealthChecksBuilderAddCheckExtensions.cs"/>
    /// </summary>
    internal sealed class HealthChecksBuilder : IHealthChecksBuilder
    {
        private readonly IServiceCollection _services;

        public HealthChecksBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            _services = services;
        }

        public IServiceCollection Services => _services;


        /// <summary>
        /// Add Health Check
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheck<TService>(string name)
            where TService : class, IHealthCheck
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            CreateRegistration(typeof(TService), name);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, TService>());
            return this;
        }


        /// <summary>
        /// Add Health Check
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheck<TService>(string name, IEnumerable<string> tags)
            where TService : class, IHealthCheck
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            CreateRegistration(typeof(TService), name, tags);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, TService>());
            return this;
        }



        /// <summary>
        /// Add Health Check
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddCheck(IHealthCheck instance, string name)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            CreateRegistration(instance.GetType(), name);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck>(instance));
            return this;
        }

        public IHealthChecksBuilder AddCheck(IHealthCheck instance, string name, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            CreateRegistration(instance.GetType(), name, tags);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck>(instance));
            return this;
        }


        /// <summary>
        /// Add check that will always return healthy
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddDefaultHealthyCheck(string name)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            CreateRegistration(typeof(DefaultHealthyCheck), name);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, DefaultHealthyCheck>());
            return this;
        }

        /// <summary>
        /// Add check that will always return healthy
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddDefaultHealthyCheck(string name, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            CreateRegistration(typeof(DefaultHealthyCheck), name, tags);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, DefaultHealthyCheck>());
            return this;
        }


        /// <summary>
        /// Add check that will always return Degraded
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddDefaultDegradedCheck(string name)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            CreateRegistration(typeof(DefaultDegradedCheck), name);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, DefaultDegradedCheck>());
            return this;
        }

        /// <summary>
        /// Add check that will always return Degraded
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddDefaultDegradedCheck(string name, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            CreateRegistration(typeof(DefaultDegradedCheck), name, tags);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, DefaultDegradedCheck>());
            return this;
        }


        /// <summary>
        /// Add check that will always return Unhealthy
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddDefaultUnhealthyCheck(string name)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            CreateRegistration(typeof(DefaultUnhealthyCheck), name);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, DefaultUnhealthyCheck>());
            return this;
        }

        /// <summary>
        /// Add check that will always return Unhealthy
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IHealthChecksBuilder AddDefaultUnhealthyCheck(string name, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            CreateRegistration(typeof(DefaultUnhealthyCheck), name, tags);
            _services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheck, DefaultUnhealthyCheck>());
            return this;
        }




        private HealthCheckRegistration CreateRegistration<T>(string name)
            => CreateRegistration(typeof(T), name);


        private HealthCheckRegistration CreateRegistration(Type type, string name)
        {
            HealthCheckRegistration registration = new HealthCheckRegistration()
            {
                Type = type,
                Name = name,
            };

            RegistrationRepository.Add(registration);

            return registration;
        }


        private HealthCheckRegistration CreateRegistration<T>(string name, IEnumerable<string> tags)
            => CreateRegistration(typeof(T), name, tags);


        private HealthCheckRegistration CreateRegistration(Type type, string name, IEnumerable<string> tags)
        {
            HealthCheckRegistration registration = new HealthCheckRegistration()
            {
                Type = type,
                Name = name,
                Tags = tags,
            };

            RegistrationRepository.Add(registration);

            return registration;
        }



        private void RemoveRegistration<T>()
            => RemoveRegistration(typeof(T));

        private void RemoveRegistration(Type type)
        {
            RegistrationRepository.Remove(type);
        }


        /// <summary>
        /// Remove Non-Keyed Service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        private void RemoveService<T>()
            => RemoveService(typeof(T));

        /// <summary>
        /// Remove Non-Keyed Service
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ReadOnlyException"></exception>
        private void RemoveService(Type type)
        {
            if (_services.IsReadOnly)
                throw new ReadOnlyException("ServiceCollection is read only");

            var serviceDescriptor = _services.Where(x => x.ServiceType == type)
                                             .FirstOrDefault();

            if (serviceDescriptor != null)
                _services.Remove(serviceDescriptor);
        }


        /// <summary>
        /// Remove Keyed Service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        private void RemoveService<T>(object key)
            => RemoveService(typeof(T), key);


        /// <summary>
        /// Remove Keyed Service
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <exception cref="ReadOnlyException"></exception>
        private void RemoveService(Type type, object key)
        {
            if (_services.IsReadOnly)
                throw new ReadOnlyException("ServiceCollection is read only");

            // This is for keyed services
            var serviceDescriptor = _services.Where(x => x.ServiceType == typeof(IHealthCheck) &&
                                                         x.ServiceKey == key &&
                                                         x.KeyedImplementationType == type) 
                                             .FirstOrDefault();

            if (serviceDescriptor != null) 
                _services.Remove(serviceDescriptor);
        }
    }
}
