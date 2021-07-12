using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Web.ServiceAuthentication
{
    public interface IServiceAuthenticationHelper
    {
        /// <summary>
        /// Expects authentication settings from Fundamentals/Configuration under json attribute "{client name}-authentication".
        ///
        /// Supports:
        ///
        /// "smoke-testing-company-authentication": {
        ///     "Type": "JwtFromUrl",
        ///     "PostUrl": "http://example.com",
        ///     "PostBody": "{ 'ClientId': 'client', 'ClientSecret': 'pwd' }",
        ///     "ResponseTokenJsonPath": "AccessToken",
        ///     "TokenCacheInMinutes": 180
        /// }
        /// Which fetches a JWT from an url and caches it for TokenCacheInMinutes
        /// 
        /// "smoke-testing-company-authentication": {
        ///     "Type": "Basic",
        ///     "Username": "uname",
        ///     "Password": "pwd",
        ///     "TokenCacheInMinutes": 10000
        /// }
        /// Which is basic authentication.
        /// 
        /// "smoke-testing-company-authentication": {
        ///     "Type": "BearerToken",
        ///     "Token": "jwt token",
        ///     "TokenCacheInMinutes": 525600
        /// }
        /// Which provides a long lived bearer token to use.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthorizationToken"/>
        /// or null if <paramref name="configuration"/> does not contain "{client name}-authentication"
        /// or null if <see cref="ClientAuthorizationSettings.AuthorizationType"/> is None.
        /// </returns>
        Task<AuthorizationToken> GetAuthorizationForClientAsync(Tenant tenant, ILeverConfiguration configuration, string client, CancellationToken cancellationToken = default);

        /// <summary>
        /// Provide your own <see cref="ClientAuthorizationSettings"/> and get the <see cref="AuthorizationToken"/> from that.
        /// </summary>
        Task<AuthorizationToken> GetAuthorizationForClientAsync(Tenant tenant, ClientAuthorizationSettings authSettings, string client, CancellationToken cancellationToken = default);
    }
}
