using System;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// The server has accepted the request and will execute it in the background. The 
    /// </summary>
    public class RequestAcceptedException : Exception
    {
        /// <summary>
        /// The id that represents this request.
        /// </summary>
        public string RequestId { get; }
        
        /// <summary>
        /// The Url where the response will be made available once it has completed.
        /// </summary>
        public string PollingUrl { get; set; }

        /// <summary>
        /// The Url where the you can register for a callback when the response is available.
        /// </summary>
        public string RegisterCallbackUrl { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestAcceptedException(string requestId)
        {
            RequestId = requestId;
        }
    }
}
