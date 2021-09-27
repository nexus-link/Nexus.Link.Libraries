﻿using System.Threading;
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
        /// To read the response. for the corresponding request
        /// </summary>
        Task<HttpResponse> ReadResponseAsync(string requestId, CancellationToken token = default);
    }
}