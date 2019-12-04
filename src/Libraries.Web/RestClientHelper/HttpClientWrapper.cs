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

        private bool _simulateOutgoingCalls;
        /// <inheritdoc />
        public bool SimulateOutgoingCalls
        {
            get => _simulateOutgoingCalls;
            set
            {
                InternalContract.Require(FulcrumApplication.IsInDevelopment,
                    $"The property {nameof(SimulateOutgoingCalls)} can only be set when {nameof(FulcrumApplication)}.{nameof(FulcrumApplication.IsInDevelopment)} is true.");
                _simulateOutgoingCalls = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient">The real HttpClient to use</param>
        public HttpClientWrapper(HttpClient httpClient)
        {
            ActualHttpClient = httpClient;
            if (ActualHttpClient == null)
            {
                InternalContract.Require(FulcrumApplication.IsInDevelopment,
                    $"The parameter {nameof(httpClient)} can only be null when {nameof(FulcrumApplication)}.{nameof(FulcrumApplication.IsInDevelopment)} is true.");
            }
            SimulateOutgoingCalls = ActualHttpClient == null;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (FulcrumApplication.IsInDevelopment && SimulateOutgoingCalls)
            {
                Log.LogInformation($"Request will be simulated : {request.ToLogString()}");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = null,
                    RequestMessage = request
                };
            }

            FulcrumAssert.IsNotNull(ActualHttpClient);
            return await ActualHttpClient.SendAsync(request, cancellationToken);
        }
    }
}