using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Logging.Data;

namespace Nexus.Link.Libraries.Web.Logging
{
    public static class LogDataWebApiExtensions
    {
        /// <summary>
        /// Collect relevant data based on the <paramref name="request"/>.
        /// </summary>
        public static async Task<HttpRequestData> ToLogDataAsync(this HttpRequestMessage request, string route = null)
        {
            var data = new HttpRequestData
            {
                Method = request.Method.ToString(),
                Path = request.RequestUri.PathAndQuery,
                Route = route ?? request.RequestUri.AbsolutePath
            };
            if (FulcrumApplication.IsInProductionOrProductionSimulation) return data;
            data.Headers = ToLogData(request.Headers);
            data.Body = await ToLogDataAsync(request.Content);
            return data;
        }

        private static Dictionary<string, string[]> ToLogData(HttpHeaders headers)
        {
            var data = new Dictionary<string, string[]>();
            foreach (var header in headers)
            {
                data.Add(header.Key, header.Value.ToArray());
            }

            return data;
        }

        private static async Task<string> ToLogDataAsync(HttpContent content)
        {
            await content.LoadIntoBufferAsync();
            return await content.ReadAsStringAsync();
        }

        /// <summary>
        /// Collect relevant data based on the <paramref name="response"/>.
        /// </summary>
        public static async Task<HttpResponseData> ToLogDataAsync(this HttpResponseMessage response, double? elapsedSeconds = null)
        {
            var data = new HttpResponseData
            {
                StatusCode = (int)response.StatusCode,
                ElapsedSeconds = elapsedSeconds
            };
            if (FulcrumApplication.IsInProductionOrProductionSimulation) return data;
            data.Headers = ToLogData(response.Headers);
            data.Body = await ToLogDataAsync(response.Content);
            return data;
        }

        /// <summary>
        /// Collect relevant data based on a <paramref name="request"/> and a <paramref name="response"/>.
        /// </summary>
        public static async Task<HttpOperationData> ToLogDataAsync(this HttpRequestMessage request, HttpResponseMessage response, string route = null, double? elapsedSeconds = null)
        {
            var requestTask = request.ToLogDataAsync(route);
            var responseTask = response.ToLogDataAsync(elapsedSeconds);
            var data = new HttpOperationData()
            {
                Request = await requestTask,
                Response = await responseTask
            };
            return data;
        }
    }
}
