using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Adds a Nexus Iam User Authorization header to all outgoing requests.
    /// </summary>
    public class AddUserAuthorization : DelegatingHandler
    {
        private readonly IContextValueProvider _provider;

        public AddUserAuthorization()
        {
            _provider = new AsyncLocalContextValueProvider();
        }

        /// <summary>
        /// Use this in unit tests to inject code that will be called to simulate SendAsync().
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public SendAsyncDelegate UnitTest_SendAsyncDependencyInjection { get; set; }
        public delegate Task<HttpResponseMessage> SendAsyncDelegate(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a Nexus Iam User Authorization header to the request before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userAuthorization = _provider.GetValue<string>(Constants.NexusUserAuthorizationKeyName);

            if (!string.IsNullOrWhiteSpace(userAuthorization))
            {
                if (!request.Headers.TryGetValues(Constants.NexusUserAuthorizationHeaderName, out _))
                {
                    request.Headers.Add(Constants.NexusUserAuthorizationHeaderName, userAuthorization);
                }
            }

            HttpResponseMessage response;
            if (UnitTest_SendAsyncDependencyInjection == null)
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            else
            {
                // This is for unit testing
                FulcrumAssert.IsTrue(FulcrumApplication.IsInDevelopment);
                response = await UnitTest_SendAsyncDependencyInjection(request, cancellationToken);
            }
            return response;
        }
    }
}