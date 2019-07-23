using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
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
    public class ExceptionToFulcrumResponse : CompatibilityDelegatingHandler
    {
        private static readonly DelegateState DelegateState = new DelegateState(typeof(ExceptionToFulcrumResponse).FullName);

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
        public ExceptionToFulcrumResponse(RequestDelegate next)
            : base(next)
        {
        }
#else
        public ExceptionToFulcrumResponse()
        {
        }
#endif
        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            DelegateState.HasStarted = true;
            try
            {
                await CallNextDelegateAsync(context);
            }
            catch (Exception exception)
            {
                await ConvertExceptionToResponseAsync(context, exception);
            }
        }

        private static async Task ConvertExceptionToResponseAsync(CompabilityInvocationContext context, Exception exception)
        {
#if NETCOREAPP
            var response = AspNetExceptionConverter.ToContentResult(exception);
            FulcrumAssert.IsTrue(response.StatusCode.HasValue);
            Debug.Assert(response.StatusCode.HasValue);
#else
            var response = AspNetExceptionConverter.ToHttpResponseMessage(exception);
#endif
#if NETCOREAPP
            context.Context.Response.StatusCode = response.StatusCode.Value;
            context.Context.Response.ContentType = response.ContentType;
            await context.Context.Response.WriteAsync(response.Content);
#else
            context.ResponseMessage = response;
            await Task.CompletedTask;
#endif
        }
    }
#if NETCOREAPP
    public static class ExceptionToFulcrumResponseExtension
    {
        public static IApplicationBuilder UseNexusExceptionToFulcrumResponse(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionToFulcrumResponse>();
        }
    }
#endif
}
