using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

#if NETCOREAPP
using Nexus.Link.Libraries.Web.AspNet.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#else
using Nexus.Link.Libraries.Web.Logging;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// Logs requests and responses in the pipe
    /// </summary>
    public class LogRequestAndResponse : CompatibilityDelegatingHandler
    {
        private static readonly DelegateState DelegateState = new DelegateState(typeof(LogRequestAndResponse).FullName);

        /// <summary>
        /// True if this delegate has started in the current context
        /// </summary>
        public static bool HasStarted
        {
            get => DelegateState.HasStarted;
            private set => DelegateState.HasStarted = value;
        }

#if NETCOREAPP
        /// <inheritdoc />
        public LogRequestAndResponse(RequestDelegate next) 
        :base(next)
        {
        }
#else
        public LogRequestAndResponse()
        {
        }
#endif
        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(LogResponseAsync)} has already been started in this http request.");
            InternalContract.Require(!ExceptionToFulcrumResponse.HasStarted,
                $"{nameof(ExceptionToFulcrumResponse)} must not precede {nameof(LogRequestAndResponse)}");
            DelegateState.HasStarted = true;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                await CallNextDelegateAsync(context);
                stopWatch.Stop();
                await LogResponseAsync(context, stopWatch.Elapsed);
            }
            catch (Exception exception)
            {
                // If ExceptionToFulcrumResponse handler is used, we should not end up here.
                stopWatch.Stop();
                LogException(context, exception, stopWatch.Elapsed);
                throw;
            }
        }

        private static async Task LogResponseAsync(CompabilityInvocationContext context, TimeSpan elapsedTime)
        {
            var logLevel = LogSeverityLevel.Information;
#if NETCOREAPP
            var request = context.Context.Request;
            var response = context.Context.Response;
            if (response.StatusCode >= 500) logLevel = LogSeverityLevel.Error;
            else if (response.StatusCode >= 400) logLevel = LogSeverityLevel.Warning;
#else
            var request = context.RequestMessage;
            var response = context.ResponseMessage;
            if ((int)response.StatusCode >= 500) logLevel = LogSeverityLevel.Error;
            else if ((int)response.StatusCode >= 400) logLevel = LogSeverityLevel.Warning;
#endif
            Log.LogOnLevel(logLevel, $"INBOUND request-response {await request.ToLogStringAsync(response, elapsedTime)}");
        }

        private static void LogException(CompabilityInvocationContext context, Exception exception, TimeSpan elapsedTime)
        {
#if NETCOREAPP
            var request = context.Context.Request;
#else
            var request = context.RequestMessage;
#endif
            Log.LogError($"INBOUND request-exception {request.ToLogString(elapsedTime)} | {exception.Message}", exception);
        }
    }
#if NETCOREAPP
    public static class LogRequestAndResponseExtension
    {
        public static IApplicationBuilder UseNexusLogRequestAndResponse(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogRequestAndResponse>();
        }
    }
#endif
}
