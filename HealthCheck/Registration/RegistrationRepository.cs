using System;
using System.Collections.Concurrent;


namespace HealthCheck.Registration
{
    /// <summary>
    /// A simple way of storing default descriptions for custom health check services
    /// </summary>
    /// <remarks>
    /// Nothing fancy is needed, not even memory caching, because it should be a one time set with multiple gets
    /// </remarks>
    internal static class RegistrationRepository
    {
        private static ConcurrentDictionary<Tuple<string, HealthCheckType>, HealthCheckRegistration> _dict = 
            new ConcurrentDictionary<Tuple<string, HealthCheckType>, HealthCheckRegistration>();


        public static void Add(HealthCheckRegistration registration)
            => _dict.TryAdd(CreateKey(registration.Type, registration.HealthCheckType), registration);


        public static HealthCheckRegistration? Get<T>(HealthCheckType healthCheckType)
            => Get(typeof(T), healthCheckType);

        public static HealthCheckRegistration? Get(Type type, HealthCheckType healthCheckType) 
            => _dict.GetValueOrDefault(CreateKey(type, healthCheckType), null);


        public static void Remove<T>(HealthCheckType healthCheckType)
            => Remove(typeof(T), healthCheckType);

        public static void Remove(Type type, HealthCheckType healthCheckType)
            => _dict.Remove(CreateKey(type, healthCheckType), out HealthCheckRegistration value);


        private static Tuple<string, HealthCheckType> CreateKey(Type type, HealthCheckType healthCheckType)
            => new Tuple<string, HealthCheckType>(type.AssemblyQualifiedName, healthCheckType);
    }
}
