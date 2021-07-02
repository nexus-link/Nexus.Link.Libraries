using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;
using Nexus.Link.Libraries.Web.AspNet.Queue;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync
{
    public interface IExecuteAsync
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
    }
}