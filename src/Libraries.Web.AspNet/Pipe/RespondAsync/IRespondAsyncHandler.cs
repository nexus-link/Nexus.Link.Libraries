#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync
{
    /// <summary>
    /// The main driver for respond async.
    /// </summary>
    public interface IRespondAsyncHandler: IGetActionResult, IAlreadyRunningAsynchronously
    {
        /// <summary>
        /// Enqueue one request for eventual execution.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Guid> EnqueueAsync(HttpRequest httpRequest, CancellationToken cancellationToken);
    }
}
#endif