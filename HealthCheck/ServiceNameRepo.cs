using System;
using System.Collections.Concurrent;


namespace HealthCheck
{
    /// <summary>
    /// A simple way of storing default descriptions for custom health check services
    /// </summary>
    /// <remarks>
    /// Nothing fancy is needed, not even memory caching, because it should be a one time set with multiple gets
    /// </remarks>
    internal static class ServiceNameRepo
    {
        private const string NO_NAME = "No Name Given";
        private static ConcurrentDictionary<string, string> _dict = 
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string Get<T>()
            => Get(typeof(T));

        public static string Get(Type type) 
            => _dict.GetValueOrDefault(type.AssemblyQualifiedName, NO_NAME);

        public static void Add<T>(string? name)
            => Add(typeof(T), name);

        public static void Add(Type type, string? name)
            => _dict.TryAdd(type.AssemblyQualifiedName, name ?? NO_NAME);
    }
}
