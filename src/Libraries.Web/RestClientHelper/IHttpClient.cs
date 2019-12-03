using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    public interface IHttpClient
    {
        HttpClient ActualHttpClient { get; }
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
