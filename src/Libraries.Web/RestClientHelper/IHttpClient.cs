using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    public interface IHttpClient
    {
        HttpClient ActualHttpClient { get; }

        /// <summary>
        /// Use this for testing purposes, etc. When this is true, <see cref="SendAsync"/> will not do any actual calls, but instead always return status code OK and null content.
        /// </summary>
        bool SimulateOutgoingCalls { get; set; }

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
