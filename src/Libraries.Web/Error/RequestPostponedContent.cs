using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.Libraries.Web.Error
{
    public class RequestPostponedContent : IValidatable
    {
        /// <summary>
        /// True means that there was a problem, so the caller should try to call us again.
        /// </summary>
        [Obsolete($"Obsolete since 2023-09-12. Please set {nameof(TryAgainAfterMinimumSeconds)} to mark for try again. You might set it to TimeSpan.Zero for ASAP retry.")]
        public bool TryAgain { get; set; }

        /// <summary>
        /// If you set <see cref="TryAgain"/>, you may optionally set this property to
        /// give the expected number of seconds before a retry is made.
        /// </summary>
        public double? TryAgainAfterMinimumSeconds { get; set; }

        /// <summary>
        /// If there are requests that we are waiting for, they will be listed here.
        /// </summary>
        [Validation.NotNull]
        public IEnumerable<string> WaitingForRequestIds { get; set; } = new List<string>();

        /// <summary>
        /// If this is true, then we should back off, using <see cref="TryAgainAfterMinimumSeconds"/> as a starting point.
        /// Back off means that we should resend the request with longer and longer intervals, just as with temporary errors.
        /// </summary>
        public bool Backoff { get; set; }
        
        /// <summary>
        /// This value can be used instead of normal authentication to continue a postponed execution.
        /// </summary>
        public string ReentryAuthentication { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            foreach (var waitingForRequestId in WaitingForRequestIds)
            {
                FulcrumValidate.IsNotNull(waitingForRequestId, nameof(WaitingForRequestIds), errorLocation);
            }
        }
    }
}