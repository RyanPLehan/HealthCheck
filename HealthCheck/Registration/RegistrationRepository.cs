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
        private static ConcurrentDictionary<string, HealthCheckRegistration> _dict = new ConcurrentDictionary<string, HealthCheckRegistration>();


        public static void Add(HealthCheckRegistration registration)
            => _dict.TryAdd(CreateKey(registration.Type), registration);


        public static HealthCheckRegistration? Get<T>()
            => Get(typeof(T));

        public static HealthCheckRegistration? Get(Type type) 
            => _dict.GetValueOrDefault(CreateKey(type), null);


        public static void Remove<T>()
            => Remove(typeof(T));

        public static void Remove(Type type)
            => _dict.Remove(CreateKey(type), out HealthCheckRegistration value);


        private static string CreateKey(Type type)
            => type.AssemblyQualifiedName;
    }
}
