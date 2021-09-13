using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Adds a Nexus Translated User Id header to all outgoing requests.
    /// </summary>
    public class AddTranslatedUserId : DelegatingHandler
    {
        private readonly IContextValueProvider _provider;

        public AddTranslatedUserId()
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
        /// Adds a Nexus Translated User Id header to the request before sending it.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var translatedUserId = _provider.GetValue<string>(Constants.TranslatedUserIdKey);

            if (!string.IsNullOrWhiteSpace(translatedUserId))
            {
                if (!request.Headers.TryGetValues(Constants.NexusTranslatedUserIdHeaderName, out _))
                {
                    request.Headers.Add(Constants.NexusTranslatedUserIdHeaderName, translatedUserId);
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