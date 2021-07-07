#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync
{
    /// <summary>
    /// The main driver for respond async.
    /// </summary>
    public interface IRespondAsyncFilterSupport: IGetActionResult, IAlreadyRunningAsynchronously, IExecuteAsync
    {
        /// <summary>
        /// Enqueue one request for eventual execution.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Guid> EnqueueAsync(HttpRequest request, CancellationToken cancellationToken = default);
    }
}
#endif