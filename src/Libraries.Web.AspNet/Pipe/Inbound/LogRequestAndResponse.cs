﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
            DelegateState.HasStarted = true;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                await CallNextDelegateAsync(context);
                stopWatch.Stop();
                LogResponse(context, stopWatch.Elapsed);
            }
            catch (Exception exception)
            {
                stopWatch.Stop();
                LogException(context, exception, stopWatch.Elapsed);
                throw;
            }
        }

        private static void LogResponse(CompabilityInvocationContext context, TimeSpan elapsedTime)
        {
#if NETCOREAPP
            var request = context.Context.Request;
            var response = context.Context.Response;
#else
            var request = context.RequestMessage;
            var response = context.ResponseMessage;
#endif
            Log.LogInformation($"INBOUND request-response {request.ToLogString(response, elapsedTime)}");
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
