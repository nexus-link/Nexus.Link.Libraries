using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Json;

namespace Nexus.Link.Libraries.Web.Logging
{
    /// <summary>
    /// Extensions to some non-fulcrum classes to make them implement the methods in ILoggable.
    /// </summary>
    public static class LoggableWebApiExtensions
    {
        /// <summary>
        /// Create a string based on the <paramref name="request"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpRequestMessage request) =>
            request == null ? null : $"{request.Method?.Method} {FilteredRequestUri(request.RequestUri)}";

        /// <summary>
        /// Create a string based on the <paramref name="request"/> and <paramref name="elapsedTime"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpRequestMessage request, TimeSpan elapsedTime)
        {
            if (request == null) return null;
            var message = request.ToLogString();
            if (elapsedTime != default)
            {
                var secondsAsString = elapsedTime.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
                message += $" | {secondsAsString}s";
            }
            return message;
        }

        /// <summary>
        /// Create a string based on <paramref name="request"/> and <paramref name="response"/> that is adequate for logging.
        /// </summary>
        [Obsolete("Use ToLogStringAsync() instead. Obsolete warning since 2020-06-09, error since 2021-06-09.", true)]
        public static string ToLogString(this HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsedTime = default)
        {
            if (request == null) return null;
            var message = request.ToLogString(elapsedTime);
            message += $" | {response.ToLogString()}";
            return message;
        }

        /// <summary>
        /// Create a string based on <paramref name="request"/> and <paramref name="response"/> that is adequate for logging.
        /// </summary>
        public static async Task<string> ToLogStringAsync(this HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsedTime = default, CancellationToken cancellationToken = default)
        {
            if (request == null) return null;
            var message = request.ToLogString(elapsedTime);
            message += $" | {await response.ToLogStringAsync(cancellationToken)}";
            return message;
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        [Obsolete("Use ToLogStringAsync() instead. Obsolete warning since 2020-06-09, error since 2021-06-09.", true)]
        public static string ToLogString(this HttpResponseMessage response)
        {
            return response?.StatusCode.ToLogString();
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        public static async Task<string> ToLogStringAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (response == null) return null;
            var logString = $"{(int) response.StatusCode} ({response.StatusCode})";
            if ((int) response.StatusCode < 400 || response.Content == null) return logString;
            await response.Content.LoadIntoBufferAsync();
            var body = await response.Content.ReadAsStringAsync();
            var fulcrumError = JsonHelper.SafeDeserializeObject<FulcrumError>(body);
            if (fulcrumError != null)
            {
                logString += $" | {fulcrumError.Type} | {fulcrumError.TechnicalMessage}";
            }

            return logString;
        }

        /// <summary>
        /// Create a string based on the <paramref name="statusCode"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpStatusCode statusCode) =>
            $"{(int)statusCode} {statusCode}";

        private static string FilteredRequestUri(Uri uri)
        {
            var url = uri.AbsoluteUri;
            if (string.IsNullOrWhiteSpace(url)) return "";
            var result = Regex.Replace(url, "(api_key=)[^&]+", match => match.Groups[1].Value + "hidden");
            return result;
        }
    }
}
