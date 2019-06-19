using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <inheritdoc />
    internal class ClientCredentials : ServiceClientCredentials
    {
        private readonly AuthenticationToken _token;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClientCredentials(AuthenticationToken token)
        {
            InternalContract.RequireNotNull(token, nameof(token));
            InternalContract.Require(token.Type == "Bearer", $"Parameter {nameof(token)} must be of type Bearer.");
            _token = token;
        }

        /// <inheritdoc />
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}