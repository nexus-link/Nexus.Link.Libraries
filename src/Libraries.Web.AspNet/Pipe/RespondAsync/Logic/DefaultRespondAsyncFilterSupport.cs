#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;
using Nexus.Link.Libraries.Web.AspNet.Queue;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic
{
    /// <summary>
    /// Normally you should be able to use this class without overriding it.
    /// </summary>
    public class DefaultRespondAsyncFilterSupport : IRespondAsyncFilterSupport
    {
        /// <summary>
        /// Handles execution and deciding if a request should be responded to asynchronously or not.
        /// </summary>
        public IRequestExecutor RequestExecutor { get; }

        /// <summary>
        /// Makes the final response available to the client.
        /// </summary>
        public IResponseHandler ResponseHandler { get; }

        /// <summary>
        /// The queue for the request
        /// </summary>
        public IStoppableQueue<RequestData> RequestQueue { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestQueue">The queue where incoming requests are waiting for execution.</param>
        /// <param name="requestExecutor">The code that handles the execution of a waiting request.</param>
        /// <param name="responseHandler">The code that makes the response available to the client.</param>
        public DefaultRespondAsyncFilterSupport(IStoppableQueue<RequestData> requestQueue, IRequestExecutor requestExecutor, IResponseHandler responseHandler)
        {
            RequestExecutor = requestExecutor;
            ResponseHandler = responseHandler;
            RequestQueue = requestQueue;
        }

        /// <inheritdoc />
        public virtual async Task<Guid> EnqueueAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var requestData = await new RequestData().FromAsync(request, cancellationToken);
            await RequestQueue.EnqueueAsync(requestData, cancellationToken);
            return requestData.Id;
        }

        public virtual Task<IActionResult> GetActionResultAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            return ResponseHandler.GetActionResultAsync(requestId, cancellationToken);
        }

        public virtual string GetResponseUrl(Guid requestId)
        {
            return ResponseHandler.GetResponseUrl(requestId);
        }

        /// <inheritdoc />
        public virtual bool IsRunningAsynchronously(HttpRequest request)
        {
            return RequestExecutor.IsRunningAsynchronously(request);
        }
    }
}
#endif