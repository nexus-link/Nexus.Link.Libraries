using System;
using System.Threading.Tasks;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#else
using System.Net.Http;
using System.Threading;
#endif
namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    [Obsolete("Use CompatibilityDelegatingHandlerWithCancellationSupport instead. Obsolete warning since 2021-06-09.")]
    public abstract class CompatibilityDelegatingHandler
#if NETCOREAPP

#else
        : DelegatingHandler
#endif
    {
#if NETCOREAPP
        /// <summary>
        /// The next delegate to be invoked
        /// </summary>
        protected readonly RequestDelegate _next;

        /// <inheritdoc />
        protected CompatibilityDelegatingHandler(RequestDelegate next)
        {
            _next = next;
        }

        public virtual async Task InvokeAsync(HttpContext context)
        {
            var ctx = new CompabilityInvocationContext(context);
            await InvokeAsync(ctx);
        }
#else
        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var ctx = new CompabilityInvocationContext(request, cancellationToken);
            await InvokeAsync(ctx);
            return ctx.ResponseMessage;
        }
#endif
        /// <summary>
        /// This is where the compatible invocation takes place. 
        /// </summary>
        /// <param name="context">The compatible delegate context.</param>
        protected abstract Task InvokeAsync(CompabilityInvocationContext context);

        /// <summary>
        /// Call the next delegate in the chain. 
        /// </summary>
        /// <param name="context">The compatible delegate context.</param>
        protected virtual async Task CallNextDelegateAsync(CompabilityInvocationContext context)
        {

#if NETCOREAPP
            await _next(context.Context);
#else
            var response = await base.SendAsync(context.RequestMessage, context.CancellationToken);
            context.ResponseMessage = response;
#endif
        }
    }
}
