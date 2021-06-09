using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Logging;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Logs requests and responses in the pipe
    /// </summary>
    public class LogRequestAndResponse : DelegatingHandler
    {
        /// <summary>
        /// A delegate for testing purposes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public delegate Task<HttpResponseMessage> SendAsyncDelegate(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Use this in unit tests to inject code that will be called to simulate SendAsync().
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public SendAsyncDelegate UnitTest_SendAsyncDependencyInjection { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public LogRequestAndResponse()
        {
            FulcrumApplication.ValidateButNotInProduction();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                HttpResponseMessage response;
                if (UnitTest_SendAsyncDependencyInjection == null)
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    // This is for unit testing
                    FulcrumAssert.IsTrue(FulcrumApplication.IsInDevelopment);
                    response = await UnitTest_SendAsyncDependencyInjection(request, cancellationToken);
                }
                timer.Stop();
                await LogResponseAsync(request, response, timer.Elapsed, cancellationToken);
                return response;
            }
            catch (Exception e)
            {
                timer.Stop();
                LogException(request, e, timer.Elapsed);
                throw;
            }
        }

        private async Task LogResponseAsync(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsedTime, CancellationToken cancellationToken)
        {
            if (request == null) return;
            LogSeverityLevel level;
            if (response.IsSuccessStatusCode) level = LogSeverityLevel.Information;
            else if ((int)response.StatusCode >= 500) level = LogSeverityLevel.Warning;
            else if ((int)response.StatusCode >= 400) level = LogSeverityLevel.Error;
            else level = LogSeverityLevel.Warning;
            Log.LogOnLevel(level, $"OUTBOUND request-response {await request.ToLogStringAsync(response, elapsedTime, cancellationToken: cancellationToken)}");
        }

        private void LogException(HttpRequestMessage request, Exception exception, TimeSpan elapsedTime)
        {
            Log.LogError($"OUTBOUND request-exception {request.ToLogString(elapsedTime)} | {exception.Message}", exception);
        }
    }
}
