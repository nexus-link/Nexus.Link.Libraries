using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// The server has accepted the request and will execute it in the background. The 
    /// </summary>
    public class RequestPostponedException : Exception
    {
        /// <summary>
        /// If there are any other requests that we are waiting for, they will be listed here.
        /// </summary>
        public List<string> WaitingForRequestIds { get; } = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestPostponedException(IEnumerable<string> waitingForRequestIds)
        {
            if (waitingForRequestIds != null)
            {
                WaitingForRequestIds.AddRange(waitingForRequestIds);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestPostponedException(params string[] waitingForRequestIds)
        {
            if (waitingForRequestIds != null)
            {
                WaitingForRequestIds.AddRange(waitingForRequestIds);
            }
        }

        /// <summary>
        /// Add requests that we are waiting for.
        /// </summary>
        /// <param name="waitingForRequestIds">Requests that we are waiting for</param>
        /// <returns></returns>
        public RequestPostponedException AddWaitingForIds(IEnumerable<string> waitingForRequestIds)
        {
            if (waitingForRequestIds == null) return this;
            WaitingForRequestIds.AddRange(waitingForRequestIds);
            return this;
        }

        /// <summary>
        /// Add requests that we are waiting for.
        /// </summary>
        /// <param name="waitingForRequestIds">Requests that we are waiting for</param>
        /// <returns></returns>
        public RequestPostponedException AddWaitingForIds(params string[] waitingForRequestIds)
        {
            if (waitingForRequestIds == null) return this;
            WaitingForRequestIds.AddRange(waitingForRequestIds);
            return this;
        }
    }
}
