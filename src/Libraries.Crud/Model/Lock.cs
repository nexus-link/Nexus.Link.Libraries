using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <inheritdoc />
    public class Lock<TId> : Lock<TId, TId>
    {
    }

    /// <inheritdoc />
    public class Lock<TObjectId, TLockId> : BaseLock<TLockId>
    {

        /// <summary>
        /// The id of the object that the lock is for.
        /// </summary>
        public TObjectId ItemId { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotDefaultValue(ItemId, nameof(ItemId), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ItemId} ({ValidUntil})";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return LockId.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Lock<TObjectId, TLockId> @lock)) return false;
            return Equals(LockId, @lock.LockId) && Equals(ItemId, @lock.ItemId);
        }
    }

    public static class LockExtensions
    {
        public static Lock<string> From(this Lock<string> target, Lock<Guid> source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireNotNull(target, nameof(target));
            target.LockId = source.LockId.ToGuidString();
            target.ItemId = source.ItemId.ToGuidString();
            target.ValidUntil = source.ValidUntil;
            return target;
        }
        public static Lock<Guid> From(this Lock<Guid> target, Lock<string> source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireNotNull(target, nameof(target));
            target.LockId = source.LockId.ToGuid();
            target.ItemId = source.ItemId.ToGuid();
            target.ValidUntil = source.ValidUntil;
            return target;
        }
    }
}