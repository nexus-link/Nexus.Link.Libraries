namespace Nexus.Link.Libraries.Core.Platform.Authentication
{
    /// <summary>
    /// Credentials for authentication.
    /// </summary>
    public class AuthenticationCredentials : IAuthenticationCredentials
    {
        /// <inheritdoc />
        public string ClientId { get; set; }

        /// <inheritdoc />
        public string ClientSecret { get; set; }
    }
}
