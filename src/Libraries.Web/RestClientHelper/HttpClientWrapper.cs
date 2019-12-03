using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Logging;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// This class is used in conjunction with <see cref="IHttpClient"/> to add an interface to the <see cref="ActualHttpClient"/> class.
    /// Use this class instead of using <see cref="ActualHttpClient"/> to create a client and use the <see cref="IHttpClient"/> interface to reference it.
    /// </summary>
    public class HttpClientWrapper : IHttpClient
    {
        public HttpClient ActualHttpClient { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient">The real HttpClient to use</param>
        public HttpClientWrapper(HttpClient httpClient)
        {
            ActualHttpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (FulcrumApplication.IsInDevelopment && ActualHttpClient == null)
            {
                Log.LogInformation($"Request was swallowed because the application has run time level Development: {request.ToLogString()}");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = null,
                    RequestMessage =  request
                };
            }
            else
            {
                FulcrumAssert.IsNotNull(ActualHttpClient);
                return await ActualHttpClient.SendAsync(request, cancellationToken);
            }
        }
    }
}