using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <summary>
    /// Information about a claimed lock
    /// </summary>
    public abstract class BaseLock<TId> : IValidatable
    {
        /// This identifies the lock and anyone that has this id in their possession is considered the owner of the lock.
        public TId LockId { get; set; }

        /// <summary>
        /// The id of the object that the lock is for.
        /// </summary>
        public DateTimeOffset ValidUntil { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(LockId, nameof(LockId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(ValidUntil, nameof(ValidUntil), errorLocation);
        }
    }
}