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
    public class RespondAsyncHandler : BackgroundService, IRespondAsyncHandler
    {
        /// <summary>
        /// Handles execution and deciding if a request should be responded to asynchronously or not.
        /// </summary>
        protected IRequestExecutor RequestExecutor { get; }

        /// <summary>
        /// Makes the final response available to the client.
        /// </summary>
        protected IResponseHandler ResponseHandler { get; }

        private readonly IStoppableQueue<RequestData> _requestQueue;

        public RespondAsyncHandler(IStoppableQueue<RequestData> requestQueue, IRequestExecutor requestExecutor, IResponseHandler responseHandler)
        {
            RequestExecutor = requestExecutor;
            ResponseHandler = responseHandler;
            _requestQueue = requestQueue;
        }

        /// <inheritdoc />
        public virtual async Task<Guid> EnqueueAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            var requestData = await new RequestData().FromAsync(httpRequest, cancellationToken);
            await _requestQueue.EnqueueAsync(requestData, cancellationToken);
            return requestData.Id;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var requestData = await _requestQueue.DequeueAsync(cancellationToken);

                try
                {
                    var responseData = await RequestExecutor.ExecuteRequestAsync(requestData, cancellationToken);
                    await ResponseHandler.AddResponse(requestData, responseData);
                }
                catch (Exception e)
                {
                    // The method above should never fail, and if it does we will just log that and ignore the error
                    Log.LogError($"The method {nameof(IRequestExecutor)}.{nameof(IRequestExecutor.ExecuteRequestAsync)} should never throw an exception, but it did.:\r{e.Message}", e);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            var task = _requestQueue.StopAsync(stoppingToken);
            await base.StopAsync(stoppingToken);
            await task;
        }

        public virtual Task<IActionResult> GetActionResultAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            return ResponseHandler.GetActionResultAsync(requestId, cancellationToken);
        }

        /// <inheritdoc />
        public bool IsRunningAsynchronously(HttpRequest request)
        {
            return RequestExecutor.IsRunningAsynchronously(request);
        }
    }
}
#endif