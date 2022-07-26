using System;
using System.Diagnostics;
using System.Threading;
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
    public class LogRequestAndResponse : CompatibilityDelegatingHandlerWithCancellationSupport
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
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public LogRequestAndResponse(RequestDelegate next) 
        :base(next)
        {
        }
#else
        public LogRequestAndResponse()
        {
        }
#endif
        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(LogResponseAsync)} has already been started in this http request.");
            InternalContract.Require(!ExceptionToFulcrumResponse.HasStarted,
                $"{nameof(ExceptionToFulcrumResponse)} must not precede {nameof(LogRequestAndResponse)}");
            DelegateState.HasStarted = true;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await CallNextDelegateAsync(context, cancellationToken);
                stopwatch.Stop();
                await LogResponseAsync(context, stopwatch.Elapsed, cancellationToken);
            }
            catch (Exception exception)
            {
                // If ExceptionToFulcrumResponse handler is used, we should not end up here.
                stopwatch.Stop();
                LogException(context, exception, stopwatch.Elapsed);
                throw;
            }
        }

        private static async Task LogResponseAsync(CompabilityInvocationContext context, TimeSpan elapsedTime, CancellationToken cancellationToken)
        {
            var logLevel = LogSeverityLevel.Information;
#if NETCOREAPP
            var request = context.Context.Request;
            var response = context.Context.Response;
            logLevel = CalculateLogSeverityLevel(response.StatusCode);
#else
            var request = context.RequestMessage;
            var response = context.ResponseMessage;
            logLevel = CalculateLogSeverityLevel((int)response.StatusCode);
#endif
            Log.LogOnLevel(logLevel, $"INBOUND request-response {await request.ToLogStringAsync(response, elapsedTime, cancellationToken: cancellationToken)}");

            LogSeverityLevel CalculateLogSeverityLevel(int statusCode)
            {
                var level = statusCode switch
                {
                    502 => LogSeverityLevel.Warning,
                    >= 500 => LogSeverityLevel.Error,
                    423 => LogSeverityLevel.Information,
                    >= 400 => LogSeverityLevel.Warning,
                    _ => LogSeverityLevel.Information
                };
                return level;
            }
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
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public static IApplicationBuilder UseNexusLogRequestAndResponse(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogRequestAndResponse>();
        }
    }
#endif
}
