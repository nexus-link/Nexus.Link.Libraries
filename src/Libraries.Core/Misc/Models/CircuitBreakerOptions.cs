using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class CircuitBreakerOptions : IValidatable
    {
        /// <summary>
        /// The cool down strategy. This is used to decide when it is time to retry the resource again.
        /// </summary>
        public ICoolDownStrategy CoolDownStrategy { get; set; }

        /// <summary>
        /// If this is true, whenever one call fails, then all concurrent calls will get a cancellation request.
        /// </summary>
        public bool CancelConcurrentRequestsWhenOneFails { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(CoolDownStrategy, nameof(CoolDownStrategy), errorLocation);
        }
    }
}