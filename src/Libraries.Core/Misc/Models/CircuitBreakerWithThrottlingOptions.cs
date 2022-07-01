using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class CircuitBreakerWithThrottlingOptions : CircuitBreakerOptions
    {
        /// <summary>
        /// The cool down strategy. This is used to decide when it is time to increase the level of concurrency.
        /// </summary>
        public ICoolDownStrategy ThrottlingCoolDownStrategy { get; set; }

        /// <summary>
        /// When this number of concurrency has been reached, we will consider the choking situation to be resolved.
        /// </summary>
        public int ConcurrencyThresholdForChokingResolved { get; set; }

        /// <inheritdoc cref="IValidatable" />
        public new void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNull(ThrottlingCoolDownStrategy, nameof(ThrottlingCoolDownStrategy), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(2, ConcurrencyThresholdForChokingResolved, nameof(ConcurrencyThresholdForChokingResolved), errorLocation);
        }
    }
}