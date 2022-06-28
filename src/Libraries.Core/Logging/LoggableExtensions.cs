using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Extensions to some non-fulcrum classes to make them implement the methods in ILoggable.
    /// </summary>
    public static class LoggableExtensions
    {
        /// <summary>
        /// Makes a <see cref="IAsyncLogger"/> into a <see cref="ISyncLogger"/> by adding a memory queue.
        /// </summary>
        public static ISyncLogger AsQueuedSyncLogger(this IAsyncLogger value) => new QueueToAsyncLogger(value);

        /// <summary>
        /// Adds a <see cref="BatchLogger"/> around a <see cref="ISyncLogger"/>.
        /// </summary>
        public static ISyncLogger AsBatchLogger(this ISyncLogger value) => new BatchLogger(value);

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this DateTimeOffset value) =>
            value.ToString("o", CultureInfo.InvariantCulture);

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this TimeSpan value)
        {
            if (value < TimeSpan.FromMilliseconds(1000)) return $"{value.TotalMilliseconds:F0}ms";
            if (value < TimeSpan.FromMilliseconds(1995)) return $"{value.TotalSeconds:F2}s";
            if (value < TimeSpan.FromSeconds(9.951)) return $"{value.TotalSeconds:F1}s";
            if (value < TimeSpan.FromSeconds(30)) return $"{Math.Floor(value.TotalSeconds * 2.0) / 2.0:F1}s";
            if (value < TimeSpan.FromSeconds(90)) return $"{value.TotalSeconds:F0}s";
            if (value < TimeSpan.FromMinutes(10)) return $"{value.Minutes}min {value.Seconds}s";
            if (value < TimeSpan.FromMinutes(30)) return $"{Math.Floor(value.TotalMinutes * 2.0) / 2.0:F1}min";
            if (value < TimeSpan.FromMinutes(90)) return $"{value.TotalMinutes:F0}min";
            if (value < TimeSpan.FromHours(12)) return $"{value.Hours}h {value.Minutes}min";
            if (value < TimeSpan.FromHours(24)) return $"{Math.Floor(value.TotalHours * 2.0) / 2.0:F1}h";
            if (value < TimeSpan.FromDays(7)) return $"{value.Days} days {value.Hours}h";
            if (value < TimeSpan.FromDays(14)) return $"{Math.Floor(value.TotalDays*2.0)/2.0:F1} days";
            return $"{value.TotalDays:F0} days";
        }

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this JToken value) =>
            value.ToString(Formatting.Indented);

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this JObject value) =>
            value.ToString(Formatting.Indented);

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this JValue value) =>
            value.ToString(Formatting.Indented);

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this JArray values)
        {
            var allStrings = values.Select(v => v.ToLogString());
            var oneString = string.Join(", ", allStrings);
            return $"({oneString})";
        }

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString<T>(this IEnumerable<T> values)
            where T : ILoggable
        {
            var allStrings = values.Select(v => v.ToLogString());
            var oneString = string.Join(", ", allStrings);
            return $"({oneString})";
        }

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this Exception value)
        {
            return value.ToLogString(false);
        }

        /// <summary>
        /// Very much like <see cref="object.ToString"/>, but specifically for logging purposes.
        /// </summary>
        /// <param name="value">The exception to base the string on.</param>
        /// <param name="hideStackTrace">When this is true, any stack trace will be hidden.</param>
        /// <returns>A string for logging information about this type of object.</returns>
        public static string ToLogString(this Exception value, bool hideStackTrace)
        {
            if (value == null) return "";
            try
            {
                var formatted = $"Exception type: {value.GetType().FullName}";
                if (value is FulcrumException fulcrumValue) formatted += $"\r{fulcrumValue.ToLogString()}";
                else formatted += $" | Exception message:\r{value.Message}";
                if (!hideStackTrace) formatted += $"\rStack trace:\r{value.StackTrace}";
                formatted += AddInnerExceptions(value, hideStackTrace);
                return formatted;
            }
            catch (Exception)
            {
                return value.Message;
            }
        }

        private static string AddInnerExceptions(Exception exception, bool hideStackTrace)
        {
            var formatted = "";
            if (exception is AggregateException aggregateException)
            {
                formatted += "\rAggregated exceptions:";
                formatted = aggregateException
                    .Flatten()
                    .InnerExceptions
                    .Aggregate(formatted, (current, innerException) => current + $"\r{innerException.ToLogString(hideStackTrace)}");
            }
            if (exception.InnerException != null)
            {
                formatted += $"\r--Inner exception--\r{exception.InnerException.ToLogString(hideStackTrace)}";
            }
            return formatted;
        }
    }
}
