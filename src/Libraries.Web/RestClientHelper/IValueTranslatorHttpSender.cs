﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

#pragma warning disable 1591

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    public interface IValueTranslatorHttpSender : IHttpSender
    {
        /// <summary>
        /// Send a request with method <paramref name="method"/> to <paramref name="relativeUrl"/> with <paramref name="body"/> and decorate the resulting string.
        /// </summary>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="method">POST, GET, etc.</param>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The body (content) for the request.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        Task<HttpOperationResponse<string>> SendRequestAndDecorateResponseAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
