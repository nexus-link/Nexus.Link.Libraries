using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.Error
{
    public class RequestPostponedContent : IValidatable
    {
        /// <summary>
        /// True means that there was a problem, so the caller should try to call us again.
        /// </summary>
        public bool TryAgain { get; set; }

        /// <summary>
        /// If there are requests that we are waiting for, they will be listed here.
        /// </summary>
        public IEnumerable<string> WaitingForRequestIds { get; set; } = new List<string>();

        /// <summary>
        /// This value can be used instead of normal authentication to continue a postponed execution.
        /// </summary>
        public string ReentryAuthentication { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(WaitingForRequestIds, nameof(WaitingForRequestIds), errorLocation);
            foreach (var waitingForRequestId in WaitingForRequestIds)
            {
                FulcrumValidate.IsNotNull(waitingForRequestId, nameof(WaitingForRequestIds), errorLocation);
            }
        }
    }
}