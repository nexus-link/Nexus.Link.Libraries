#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Nexus.Link.Libraries.Web.AspNet.Logging
{
    /// <summary>
    /// Extensions to some non-fulcrum classes to make them implement the methods in ILoggable.
    /// </summary>
    public static class LoggableWebAppExtensions
    {
        /// <summary>
        /// Create a string based on the <paramref name="request"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpRequest request) =>
            request == null ? null : $"{request.Method} {FilteredRequestUri(request.GetDisplayUrl())}";

        /// <summary>
        /// Create a string based on the <paramref name="request"/> and <paramref name="elapsedTime"/> that is adequate for logging.
        /// </summary>
        public static string ToLogString(this HttpRequest request, TimeSpan elapsedTime)
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
        public static string ToLogString(this HttpRequest request, HttpResponse response, TimeSpan elapsedTime = default(TimeSpan))
        {
            if (request == null) return null;
            var message = request.ToLogString(elapsedTime);
            if (response != null) message += $" | {response.StatusCode}";
            return message;
        }

        private static string FilteredRequestUri(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            var result = Regex.Replace(url, "(api_key=)[^&]+", match => match.Groups[1].Value + "hidden");
            return result;
        }
    }
}
#endif
