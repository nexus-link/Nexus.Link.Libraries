using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <inheritdoc />
    public class Lock<TId> : BaseLock<TId>
    {

        /// <summary>
        /// The id of the object that the lock is for.
        /// </summary>
        public TId ItemId { get; set; }

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
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Lock<TId> @lock)) return false;
            return Equals(Id, @lock.Id) && Equals(ItemId, @lock.ItemId);
        }
    }
}