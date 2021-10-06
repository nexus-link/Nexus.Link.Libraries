using System.Collections.Generic;

namespace Nexus.Link.Libraries.Web.Error
{
    public class AcceptedInformation
    {
        
        /// <summary>
        /// The Url where the response will be made available once it has completed.
        /// </summary>
        public string UrlWhereResponseWillBeMadeAvailable { get; set; }

        /// <summary>
        /// If there are any other requests that we are waiting for, they will be listed here.
        /// </summary>
        public List<string> OutstandingRequestIds { get; set; }
    }
}