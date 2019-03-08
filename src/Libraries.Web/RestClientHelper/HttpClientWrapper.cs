using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// This class is used in conjunction with <see cref="IHttpClient"/> to add an interface to the <see cref="HttpClient"/> class.
    /// Use this class instead of using <see cref="HttpClient"/> to create a client and use the <see cref="IHttpClient"/> interface to reference it.
    /// </summary>
    public class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient">The real HttpClient to use</param>
        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
    }
}