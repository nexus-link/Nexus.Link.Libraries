using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// The server has accepted the request and will execute it in the background. The 
    /// </summary>
    public class RequestPostponedException : Exception
    {
        /// <summary>
        /// True means that there was a problem, so the caller should try to call us again.
        /// </summary>
        public bool TryAgain { get; set; }

        /// <summary>
        /// If you set <see cref="TryAgain"/>, you may optionally set this property to
        /// give the expected time before a retry is made.
        /// </summary>
        [Validation.Null(InvertedTrigger = true, TriggerPropertyName = nameof(TryAgain))]
        public TimeSpan? TryAgainAfterMinimumTimeSpan { get; set; }

        /// <summary>
        /// If there are any other requests that we are waiting for, they will be listed here.
        /// </summary>
        [Validation.NotNull]
        public List<string> WaitingForRequestIds { get; } = new List<string>();

        /// <summary>
        /// This value can be used instead of normal authentication to continue a postponed execution.
        /// </summary>
        public string ReentryAuthentication { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestPostponedException(IEnumerable<string> waitingForRequestIds)
        {
            AddWaitingForIds(waitingForRequestIds);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestPostponedException(params string[] waitingForRequestIds)
        {
            AddWaitingForIds(waitingForRequestIds);
        }

        /// <summary>
        /// Add requests that we are waiting for.
        /// </summary>
        /// <param name="waitingForRequestIds">Requests that we are waiting for</param>
        /// <returns></returns>
        public RequestPostponedException AddWaitingForIds(IEnumerable<string> waitingForRequestIds)
        {
            if (waitingForRequestIds == null) return this;
            WaitingForRequestIds.AddRange(waitingForRequestIds.Where(ri => ri != null));
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
            WaitingForRequestIds.AddRange(waitingForRequestIds.Where(ri => ri != null));
            return this;
        }
    }
}
