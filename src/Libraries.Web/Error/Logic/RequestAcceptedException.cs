using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// The server has accepted the request and will execute it in the background. The 
    /// </summary>
    public class RequestAcceptedException : Exception
    {
        
        /// <summary>
        /// The Url where the response will be made available once it has completed.
        /// </summary>
        public string UrlWhereResponseWillBeMadeAvailable { get; }

        /// <summary>
        /// If there are any other requests that we are waiting for, they will be listed here.
        /// </summary>
        public List<string> OutstandingRequestIds { get; } = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestAcceptedException(string urlWhereResponseWillBeMadeAvailable,
            IEnumerable<string> outstandingRequestIds = null)
        {
            UrlWhereResponseWillBeMadeAvailable = urlWhereResponseWillBeMadeAvailable;
            if (outstandingRequestIds != null)
            {
                OutstandingRequestIds.AddRange(outstandingRequestIds);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestAcceptedException AddOutstandingRequestIds(IEnumerable<string> outstandingRequestIds)
        {
            if (outstandingRequestIds == null) return this;
            OutstandingRequestIds.AddRange(outstandingRequestIds);
            return this;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestAcceptedException AddOutstandingRequestId(string outstandingRequestId)
        {
            InternalContract.RequireNotNull(outstandingRequestId, nameof(outstandingRequestId));
            OutstandingRequestIds.Add(outstandingRequestId);
            return this;
        }
    }
}
