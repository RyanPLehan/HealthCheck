using System;
using System.Threading.Tasks;

namespace HealthCheck.Listeners.Web
{
    public class HealthCheckOptions
    {
        /// <summary>
        /// Gets or sets a predicate that is used to filter the set of health checks executed.
        /// </summary>
        /// <remarks>
        /// If <see cref="Predicate"/> is <c>null</c>, all registered health checks will be ran.
        /// This is the default behavior. To run a subset of health checks,
        /// provide a function that filters the set of checks.
        /// </remarks>
        public Func<HealthCheckRegistration, bool>? Predicate { get; set; }

    }
}
