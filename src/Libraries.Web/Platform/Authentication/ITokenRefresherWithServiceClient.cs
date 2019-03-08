using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Libraries.Web.Platform.Authentication
{
    /// <summary>
    /// A Service Client that can refresh tokens.
    /// </summary>
    public interface ITokenRefresherWithServiceClient : ITokenRefresher
    {
        /// <summary>
        /// Get "this" as a ServiceClient.
        /// </summary>
        ServiceClientCredentials GetServiceClient();
    }
}
