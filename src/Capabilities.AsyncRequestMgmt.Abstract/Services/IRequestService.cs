using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services
{
    /// <summary>
    /// Operations for CreateRequest and ReadResponse.
    /// </summary>
    public interface IRequestService : ICreate<HttpRequestCreate, HttpRequest, string>
    {

        /// <summary>
        /// Get endpoints related to request responses for a specific <paramref name="requestId"/>.
        /// </summary>
        /// <param name="requestId">The specific request.</param>
        /// <returns>Endpoints for polling and registering callbacks.</returns>
        RequestResponseEndpoints GetEndpoints(string requestId);
    }
}