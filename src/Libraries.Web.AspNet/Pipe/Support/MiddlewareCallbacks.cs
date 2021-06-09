#if NETCOREAPP
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Support
{
    public class MiddlewareCallbacks
    {
        /// <summary>
        /// Synchronous delegate method used before the request method has been called.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopwatch"> A stopwatch that was started when <see cref="NexusLinkMiddleware.InvokeAsync"/> started.</param>
        public delegate void BeforeRequestMethodDelegate(HttpContext context, Stopwatch stopwatch);
        
        /// <summary>
        /// Asynchronous delegate method used before the request method has been called.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopwatch"> A stopwatch that was started when <see cref="NexusLinkMiddleware.InvokeAsync"/> started.</param>
        public delegate Task BeforeRequestAsyncDelegate(HttpContext context, Stopwatch stopwatch);
        
        /// <summary>
        /// Asynchronous delegate method used before the request method has been called.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopwatch"> A stopwatch that was started when <see cref="NexusLinkMiddleware.InvokeAsync"/> started.</param>
        /// <returns>true indicates that the request method should be skipped.</returns>
        public delegate Task<bool> PreemptiveDelegateAsync(HttpContext context, Stopwatch stopwatch);

        /// <summary>
        /// Use this for setting <see cref="AsyncLocal{T}"/> values.
        /// </summary>
        public BeforeRequestMethodDelegate InitialSequentialSync { get; set; }

        /// <summary>
        /// Use this for delegates that needs to be done before the request method, and where the order of the execution
        /// of delegates is of importance.
        /// These methods will be called initially; after <see cref="InitialSequentialSync"/>.
        /// The tasks will all be started and awaited for, one at a time.
        /// </summary>
        /// <remarks>
        /// Note that you can't use async to set <see cref="AsyncLocal{T}"/> with new values; those values will be lost when the method returns.
        /// </remarks>
        public BeforeRequestAsyncDelegate InitialSequentialAsync { get; set; }

        /// <summary>
        /// Use this for delegates that needs to be done before the request method, but where the order is of no importance.
        /// These methods will be called initially; after <see cref="InitialSequentialAsync"/>.
        /// The tasks will all be started at the same time and later awaited before the middleware calls the request method.
        /// </summary>
        /// <remarks>
        /// Note that you can't use async to set <see cref="AsyncLocal{T}"/> with new values; those values will be lost when the method returns.
        /// </remarks>
        public BeforeRequestAsyncDelegate InitialConcurrentAsync { get; set; }

        /// <summary>
        /// Use this for delegates that can create a response themselves, e.g. a cache or a 202 Accepted response.
        /// The methods will be called sequentially and as soon as one method returns true, the response is considered
        /// complete and the rest of the methods will be ignored.
        /// </summary>
        public PreemptiveDelegateAsync PreemptiveSequentialAsync { get; set; }
    }
}
#endif