using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <inheritdoc />
    public class SlaveLock<TId> : BaseLock<TId>
    {

        /// <summary>
        /// The master id of the object that the lock is for.
        /// </summary>
        public TId MasterId { get; set; }

        /// <summary>
        /// The slave id of the object that the lock is for.
        /// </summary>
        public TId SlaveId { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotDefaultValue(MasterId, nameof(MasterId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(SlaveId, nameof(SlaveId), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{MasterId}/{SlaveId} ({ValidUntil})";
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
            if (!(obj is SlaveLock<TId> @lock)) return false;
            return Equals(Id, @lock.Id) && Equals(MasterId, @lock.MasterId) && Equals(SlaveId, @lock.SlaveId);
        }
    }
}