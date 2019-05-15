using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

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
            if (elapsedTime != default(TimeSpan))
            {
                message += $" | {elapsedTime.TotalSeconds:0.###} s";
            }
            return message;
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        [Obsolete("Use ToLogString(HttpRequestMessage, HttpResponseMessage, TimeSpan).")]
        public static string ToLogString(this HttpResponseMessage response, TimeSpan elapsedTime = default(TimeSpan))
        {
            if (response == null) return null;
            var message = response.RequestMessage?.ToLogString(elapsedTime);
            message += $" | {response.StatusCode.ToLogString()}";
            return message;
        }

        /// <summary>
        /// Create a string based on <paramref name="request"/> and <paramref name="response"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsedTime = default(TimeSpan))
        {
            if (request == null) return null;
            var message = request.ToLogString(elapsedTime);
            message += $" | {response.ToLogString()}";
            return message;
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpResponseMessage response)
        {
            return response?.StatusCode.ToLogString();
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
