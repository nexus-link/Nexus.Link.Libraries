using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Logic;
#if NETCOREAPP
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
    public class ExceptionToFulcrumResponse : CompatibilityDelegatingHandlerWithCancellationSupport
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
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public ExceptionToFulcrumResponse(RequestDelegate next)
            : base(next)
        {
        }
#else
        public ExceptionToFulcrumResponse()
        {
        }
#endif
        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(ExceptionToFulcrumResponse)} has already been started in this http request.");
            DelegateState.HasStarted = true;
            try
            {
                await CallNextDelegateAsync(context, cancellationToken);
            }
            catch (Exception exception)
            {
                await ConvertExceptionToResponseAsync(context, exception, cancellationToken);
            }
        }

        private static async Task ConvertExceptionToResponseAsync(CompabilityInvocationContext context, Exception exception, CancellationToken cancellationToken)
        {
#if NETCOREAPP
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, context.Context.Response, cancellationToken);
#else
            context.ResponseMessage = AspNetExceptionConverter.ToHttpResponseMessage(exception, cancellationToken);
            await Task.CompletedTask;
#endif
        }
    }
#if NETCOREAPP
    public static class ExceptionToFulcrumResponseExtension
    {
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public static IApplicationBuilder UseNexusExceptionToFulcrumResponse(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionToFulcrumResponse>();
        }
    }
#endif
}
