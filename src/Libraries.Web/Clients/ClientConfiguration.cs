using System.Collections.Generic;
using Nexus.Link.Libraries.Web.ServiceAuthentication;

namespace Nexus.Link.Libraries.Web.Clients
{
    public class ClientConfiguration
    {
        /// <summary>
        /// The unique name of the client
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A reference to <see cref="ClientAuthorizationSettings.Id"/>
        /// </summary>
        public string Authentication { get; set; }

        /// <summary>
        /// Headers to use when sending requests to this client
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; }

    }
}
