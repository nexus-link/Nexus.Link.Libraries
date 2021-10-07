using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// Support for sending HTTP requests
    /// </summary>
    public interface IHttpSender
    {
        /// <summary>
        /// The URI for the service. Separate methods in the service are called by using URL:s relative to this path.
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// Create a copy of this IHttpSender, but update the <see cref="BaseUri"/> by adding <paramref name="relativeUrl"/>.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        IHttpSender CreateHttpSender(string relativeUrl);

        /// <summary>
        /// Send a request with method <paramref name="method"/> to <paramref name="relativeUrl"/>.
        /// </summary>
        /// <param name="method">POST, GET, etc.</param>
        /// <param name="relativeUrl">The Url relative to <see cref="BaseUri"/>, including parameters, etc.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a request with method <paramref name="method"/> to <paramref name="relativeUrl"/> with <paramref name="body"/>.
        /// </summary>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="method">POST, GET, etc.</param>
        /// <param name="relativeUrl">The Url relative to <see cref="BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The body (content) for the request.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl, TBody body = default,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a request with method <paramref name="method"/> to <paramref name="relativeUrl"/> with <paramref name="body"/> and expect a result of a specific type.
        /// </summary>
        /// <typeparam name="TResponse">The type for the result.</typeparam>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="method">POST, GET, etc.</param>
        /// <param name="relativeUrl">The Url relative to <see cref="BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The body (content) for the request.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);
    }
}
