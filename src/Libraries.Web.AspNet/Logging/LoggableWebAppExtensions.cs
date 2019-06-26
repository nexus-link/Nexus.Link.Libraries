﻿#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Json;

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
                var secondsAsString = elapsedTime.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
                message += $" | {secondsAsString}s";
            }
            return message;
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        public static async Task<string> ToLogStringAsync(this HttpRequest request, HttpResponse response, TimeSpan elapsedTime = default(TimeSpan))
        {
            if (request == null) return null;
            var message = request.ToLogString(elapsedTime);
            if (response != null) message += $" | {await response.ToLogStringAsync()}";
            return message;
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        [Obsolete("Use ToLogStringAsync() instead")]
        public static string ToLogString(this HttpRequest request, HttpResponse response, TimeSpan elapsedTime = default(TimeSpan))
        {
            if (request == null) return null;
            var message = request.ToLogString(elapsedTime);
            if (response != null) message += $" | {response.StatusCode.ToString()}";
            return message;
        }

        /// <summary>
        /// Create a string based on the <paramref name="response"/> that is adequate for logging.
        /// </summary>
        public static async Task<string> ToLogStringAsync(this HttpResponse response)
        {
            if (response == null) return null;
            var logString = response.StatusCode.ToString();
            if (response.StatusCode < 400  || response.Body == null) return logString;
            var body = await GetBodyAsStringAsync(response);
            var fulcrumError = JsonHelper.SafeDeserializeObject<FulcrumError>(body);
            if (fulcrumError != null)
            {
                logString += $" | {fulcrumError.Type} | {fulcrumError.TechnicalMessage}";
            }
            return logString;
        }

        /// <summary>
        /// Read the body from a response without destroying the response
        /// </summary>
        /// <param name="response"></param>
        /// <remarks>
        /// Based on an answer for https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
        /// </remarks>
        private static async Task<string> GetBodyAsStringAsync(HttpResponse response)
        {
            var originalBody = response.Body;

            try
            {
                using (var memStream = new MemoryStream())
                {
                    response.Body = memStream;
                    memStream.Position = 0;
                    var responseBody = new StreamReader(memStream).ReadToEnd();
                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                    return responseBody;
                }

            }
            finally
            {
                response.Body = originalBody;
            }
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
