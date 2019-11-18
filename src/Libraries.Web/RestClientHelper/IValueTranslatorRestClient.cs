using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

#pragma warning disable 1591

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    public interface IValueTranslatorRestClient : IValueTranslatorHttpSender, IRestClient
    {
        /// <summary>
        /// Send POST to <paramref name="relativeUrl"/> with <paramref name="body"/> and decorate the resulting string.
        /// </summary>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The POST body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An object.</returns>
        Task<string> PostAndDecorateResultAsync<TBody>(
            string relativeUrl,
            TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
