using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.Error
{
    public class RequestPostponedContent : IValidatable
    {
        /// <summary>
        /// If there are requests that we are waiting for, they will be listed here.
        /// </summary>
        public IEnumerable<string> WaitingForRequestIds { get; set; }

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