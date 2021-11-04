using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    public class DistributedLockRecord : TimeStampedTableItem, IRecordVersion
    {
        public Guid LockId { get; set; }
        public string TableName { get; set; }
        public Guid LockedRecordId { get; set; }
        public DateTimeOffset ValidUntil { get; set; }

        /// <inheritdoc />
        public byte[] RecordVersion { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotDefaultValue(LockId, nameof(LockId), errorLocation);
            FulcrumValidate.IsNotNull(TableName, nameof(TableName), errorLocation);
            FulcrumValidate.IsNotDefaultValue(LockedRecordId, nameof(LockedRecordId), errorLocation);
        }
    }

    public static class DistributedLockExtensions
    {
        public static Lock<Guid> From(this Lock<Guid> target, DistributedLockRecord source)
        {
            target.LockId = source.LockId;
            target.ItemId = source.LockedRecordId;
            target.ValidUntil = source.ValidUntil;
            return target;
        }
    }
}