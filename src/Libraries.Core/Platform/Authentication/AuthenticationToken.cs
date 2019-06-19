using System;

namespace Nexus.Link.Libraries.Core.Platform.Authentication
{
    /// <summary>
    /// A JWT token and some metadata for that token
    /// </summary>
    public class AuthenticationToken
    {
        /// <summary>
        /// The actual token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The token type for this token
        /// </summary>
        public string Type { get; set; } = "Bearer";

        /// <summary>
        /// The time that this token expires
        /// </summary>
        public DateTimeOffset ExpiresOn { get; set; }
    }
}
