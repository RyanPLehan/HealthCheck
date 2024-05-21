using System;
using System.Collections.Generic;

namespace HealthCheck
{
    /// <summary>
    /// Represents the result of a health check
    /// </summary>
    public struct HealthCheckResult
    {
        /// <summary>
        /// Creates a new HealthCheckResult with the specified values
        /// </summary>
        /// <param name="status">
        /// A value indicating the status of the component that was checked.
        /// </param>
        /// <param name="description">
        /// A human-readable description of the status of the component that was checked. Optional.
        /// </param>
        /// <param name="exception">
        /// An Exception representing the exception that was thrown when checking for status. Optional.
        /// </param>
        /// <param name="data">
        /// Additional key-value pairs describing the health of the component. Optional.
        /// </param>
        public HealthCheckResult(HealthStatus status,
                                 string? description = default,
                                 Exception? exception = default,
                                 IReadOnlyDictionary<string, object>? data = default)
        {
            Status = status;
            Description = description;
            Exception = exception;
            Data = data;
        }

        /// <summary>
        /// Gets a value indicating the status of the component that was checked.
        /// </summary>
        public HealthStatus Status { get; private init; }

        /// <summary>
        /// Gets a human-readable description of the status of the component that was checked.
        /// </summary>
        public string Description { get; private init; }

        /// <summary>
        /// Gets an Exception representing the exception that was thrown when checking for status (if any).
        /// </summary>
        public Exception Exception { get; private init; }

        /// <summary>
        /// Gets additional key-value pairs describing the health of the component
        /// </summary>
        public IReadOnlyDictionary<string, object> Data { get; private init; }



        /// <summary>
        /// Creates a HealthCheckResult representing an unhealthy component.
        /// </summary>
        /// <param name="description">
        /// A human-readable description of the status of the component that was checked. Optional.
        /// </param>
        /// <param name="exception">
        /// An Exception representing the exception that was thrown when checking for status. Optional.
        /// </param>
        /// <param name="data">
        /// Additional key-value pairs describing the health of the component. Optional.
        /// </param>
        /// <returns>HealthCheckResult</returns>
        public static HealthCheckResult Unhealthy(string? description = default,
                                                 Exception? exception = default,
                                                 IReadOnlyDictionary<string, object>? data = default)
            => new HealthCheckResult(HealthStatus.UnHealthy, description, exception, data);


        /// <summary>
        /// Creates a HealthCheckResult representing an degraded component.
        /// </summary>
        /// <param name="description">
        /// A human-readable description of the status of the component that was checked. Optional.
        /// </param>
        /// <param name="exception">
        /// An Exception representing the exception that was thrown when checking for status. Optional.
        /// </param>
        /// <param name="data">
        /// Additional key-value pairs describing the health of the component. Optional.
        /// </param>
        /// <returns>HealthCheckResult</returns>
        public static HealthCheckResult Degraded(string? description = default,
                                                 Exception? exception = default,
                                                 IReadOnlyDictionary<string, object>? data = default)
            => new HealthCheckResult(HealthStatus.Degraded, description, exception, data);


        /// <summary>
        /// Creates a HealthCheckResult representing an healthy component.
        /// </summary>
        /// <param name="description">
        /// A human-readable description of the status of the component that was checked. Optional.
        /// </param>
        /// <param name="exception">
        /// An Exception representing the exception that was thrown when checking for status. Optional.
        /// </param>
        /// <param name="data">
        /// Additional key-value pairs describing the health of the component. Optional.
        /// </param>
        /// <returns>HealthCheckResult</returns>
        public static HealthCheckResult Healthy(string? description = default,
                                                 Exception? exception = default,
                                                 IReadOnlyDictionary<string, object>? data = default)
            => new HealthCheckResult(HealthStatus.Healthy, description, exception, data);

    }
}
