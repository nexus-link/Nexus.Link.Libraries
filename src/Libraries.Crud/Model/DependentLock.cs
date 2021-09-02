using System;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <inheritdoc />
    public class DependentLock<TId, TDependentId> : BaseLock<TId>
    {

        /// <summary>
        /// The master id of the object that the lock is for.
        /// </summary>
        public TId MasterId { get; set; }

        /// <summary>
        /// The dependent id of the object that the lock is for.
        /// </summary>
        public TDependentId DependentId { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotDefaultValue(MasterId, nameof(MasterId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(DependentId, nameof(DependentId), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{MasterId}/{DependentId} ({ValidUntil})";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is DependentLock<TId, TDependentId> @lock)) return false;
            return Equals(Id, @lock.Id) && Equals(MasterId, @lock.MasterId) && Equals(DependentId, @lock.DependentId);
        }
    }
}