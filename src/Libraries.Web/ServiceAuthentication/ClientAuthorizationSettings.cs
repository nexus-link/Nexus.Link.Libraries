namespace Nexus.Link.Libraries.Web.ServiceAuthentication
{
    /// <summary>
    /// Authorization settings for calling a client.
    ///
    /// Usually defined in Fundamentals configuration database.
    /// </summary>
    public class ClientAuthorizationSettings
    {
        public enum AuthorizationTypeEnum
        {
            /// <summary>
            /// When request doesn't need to be authenticated.
            /// Default.
            /// </summary>
            None,

            /// <summary>
            /// Authorization: Basic [token from <see cref="ClientAuthorizationSettings.Username"/> and <see cref="ClientAuthorizationSettings.Password"/>]
            /// </summary>
            Basic,
            
            /// <summary>
            /// Authorization: Bearer [<see cref="ClientAuthorizationSettings.Token"/>]
            /// </summary>
            BearerToken,
            
            /// <summary>
            /// Authorization: Bearer [token fetched from  <see cref="ClientAuthorizationSettings.PostUrl"/>]
            /// </summary>
            JwtFromUrl
        }

        /// <summary>
        /// This client's authorization type
        /// </summary>
        public AuthorizationTypeEnum AuthorizationType { get; set; }

        /// <summary>
        /// Tells for how long a token can be cached.
        /// </summary>
        /// <remarks>Defaults to 60 minutes</remarks>
        public int TokenCacheInMinutes { get; set; } = 60;

        /// <summary>
        /// For authorization type <see cref="AuthorizationTypeEnum.Basic"/>, the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// For authorization type <see cref="AuthorizationTypeEnum.Basic"/>, the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// For authorization type <see cref="AuthorizationTypeEnum.BearerToken"/>, the  token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// For authorization type <see cref="AuthorizationTypeEnum.JwtFromUrl"/>, the url to post the <see cref="PostBody"/> to
        /// </summary>
        public string PostUrl { get; set; }
        /// <summary>
        /// For authorization type <see cref="AuthorizationTypeEnum.JwtFromUrl"/>, the body to post, containing credentials
        /// </summary>
        public string PostBody { get; set; }
        /// <summary>
        /// For authorization type <see cref="AuthorizationTypeEnum.JwtFromUrl"/>, a json path to the access token of the response.
        /// E.g. "AccessToken" or "data.token".
        /// </summary>
        public string ResponseTokenJsonPath { get; set; }

        public override string ToString()
        {
            return AuthorizationType
                   + (AuthorizationType == AuthorizationTypeEnum.Basic
                       ? $": username: {Username}"
                       : "")
                   + (AuthorizationType == AuthorizationTypeEnum.BearerToken
                       ? $": token: {Token?.Substring(0, 10)}..."
                       : "")
                   + (AuthorizationType == AuthorizationTypeEnum.JwtFromUrl
                       ? $": url: {PostUrl}, json path: {ResponseTokenJsonPath}"
                       : "");
        }
    }
}