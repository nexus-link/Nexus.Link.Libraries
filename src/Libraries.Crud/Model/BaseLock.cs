using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <summary>
    /// Information about a claimed lock
    /// </summary>
    public abstract class BaseLock<TId> : IValidatable, IUniquelyIdentifiable<TId>
    {
        /// <inheritdoc />
        public TId Id { get; set; }

        /// <summary>
        /// The id of the object that the lock is for.
        /// </summary>
        public DateTimeOffset ValidUntil { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(Id, nameof(Id), errorLocation);
            FulcrumValidate.IsNotDefaultValue(ValidUntil, nameof(ValidUntil), errorLocation);
        }
    }
}