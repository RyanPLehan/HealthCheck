using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.Asserts
{
    /// <summary>
    ///   Provides a consistent means for verifying arguments and other invariants for a given
    ///   member. Because this can be shared across multiple projects, avoid using Resource files.
    /// </summary>
    public static partial class Argument
    {
        /// <summary>
        ///   Ensures that an argument's value is a string comprised of only whitespace, though
        ///   <c>null</c> is considered a valid value.  An <see cref="ArgumentException" /> is thrown
        ///   if that invariant is not met.
        /// </summary>
        ///
        /// <param name="value">The value of the argument to verify.</param>
        /// <param name="name">The name of the argument being considered.</param>
        ///
        /// <exception cref="ArgumentException">The argument is empty or contains only white-space.</exception>
        ///
        public static void AssertNotEmptyOrWhiteSpace(string value, string name)
        {
            if (value is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"The argument '{name}' may not be empty or white-space, though it may be null.", name);
            }
        }

        /// Throws if <paramref name="value"/> is null or an empty string.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static void AssertNotNullOrEmpty(string value, string name)
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }

            if (value.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty string.", name);
            }
        }


        /// <summary>
        /// Throws if <paramref name="value"/> is the default value for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of structure to validate which implements <see cref="IEquatable{T}"/>.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> is the default value for type <typeparamref name="T"/>.</exception>
        public static void AssertNotDefault<T>(ref T value, string name) where T : struct, IEquatable<T>
        {
            if (value.Equals(default))
            {
                throw new ArgumentException("Value cannot be empty.", name);
            }
        }

        /// <summary>
        /// Throws if <paramref name="value"/> is less than the <paramref name="minimum"/> or greater than the <paramref name="maximum"/>.
        /// </summary>
        /// <typeparam name="T">The type of to validate which implements <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="minimum">The minimum value to compare.</param>
        /// <param name="maximum">The maximum value to compare.</param>
        /// <param name="name">The name of the parameter.</param>
        public static void AssertInRange<T>(T value, T minimum, T maximum, string name) where T : notnull, IComparable<T>
        {
            if (minimum.CompareTo(value) > 0)
            {
                throw new ArgumentOutOfRangeException(name, "Value is less than the minimum allowed.");
            }

            if (maximum.CompareTo(value) < 0)
            {
                throw new ArgumentOutOfRangeException(name, "Value is greater than the maximum allowed.");
            }
        }

        /// <summary>
        /// Throws if <paramref name="value"/> is not defined for <paramref name="enumType"/>.
        /// </summary>
        /// <param name="enumType">The type to validate against.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not defined for <paramref name="enumType"/>.</exception>
        public static void AssertEnumDefined(Type enumType, object value, string name)
        {
            if (!Enum.IsDefined(enumType, value))
            {
                throw new ArgumentException($"Value not defined for {enumType.FullName}.", name);
            }
        }


        /// <summary>
        /// Throws if <paramref name="value"/> is null.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static void AssertNotNull<T>(T value, string name)
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws if <paramref name="value"/> has not been initialized.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> has not been initialized.</exception>
        public static void AssertNotNull<T>(T? value, string name) where T : struct
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws if <paramref name="value"/> has a value other than the <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="defaultValue">The value to validate against.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="NotSupportedException"></exception>
        public static void AssertEventHandlerNotAssigned(object value, object? defaultValue, string name)
        {
            if (value != defaultValue)
            {
                throw new NotSupportedException($"Another handler has already been assigned to {name} event and there can be only one.");
            }
        }

        /// <summary>
        /// Throws if <paramref name="value"/> has a value other than the <paramref name="expectedValue"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="expectedValue">The value to validate against.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="NotSupportedException"></exception>
        public static void AssertSameEventHandlerAssigned(object value, object? expectedValue, string name)
        {
            if (value != expectedValue)
            {
                throw new ArgumentException($"This handler has not been previously assigned to {name} event.");
            }
        }

        /// <summary>
        ///   Ensures that an instance has not been disposed, throwing an
        ///   <see cref="ObjectDisposedException" /> if that invariant is not met.
        /// </summary>
        ///
        /// <param name="value"><c>true</c> if the target instance has been disposed; otherwise, <c>false</c>.</param>
        /// <param name="name">The name of the target instance that is being verified.</param>
        ///
        public static void AssertNotDisposed(bool value, string name)
        {
            if (value)
            {
                throw new ObjectDisposedException(name, $"{name} has already been closed and cannot perform the requested operation.");
            }
        }
    }
}