using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    public class DistributedLockTable : CrudSql<DistributedLockRecord>, IDistributedLockTable
    {
        private static readonly ISqlTableMetadata Metadata = new SqlTableMetadata
        {
            TableName = "DistributedLock",
            CreatedAtColumnName = nameof(DistributedLockRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(DistributedLockRecord.RecordUpdatedAt),
            CustomColumnNames = new List<string>
            {
                nameof(DistributedLockRecord.LockId),
                nameof(DistributedLockRecord.TableName),
                nameof(DistributedLockRecord.LockedRecordId),
                nameof(DistributedLockRecord.ValidUntil)
            },
            EtagColumnName = null,
            ForeignKeyColumnName = nameof(DistributedLockRecord.LockedRecordId),
            RowVersionColumnName = nameof(DistributedLockRecord.RecordVersion),
            OrderBy = new List<string> { $"{nameof(DistributedLockRecord.RecordCreatedAt)} DESC" }
        };

        /// <inheritdoc />
        [Obsolete("Use DistributedLockTable(IDatabaseOptions, ISqlTableMetadata) instead. Obsolete since 2021-10-21.", error: false)]
        public DistributedLockTable(string connectionString) : this(new DatabaseOptions { ConnectionString = connectionString })
        {
        }

        /// <inheritdoc />
        public DistributedLockTable(IDatabaseOptions options) : base(options, Metadata)
        {
        }

        /// <inheritdoc />
        public async Task<Lock<Guid>> TryAddAsync(Guid recordToLockId, TimeSpan? lockTimeSpan = null, Guid? currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            if (lockTimeSpan == null) lockTimeSpan = Database.Options.DefaultLockTimeSpan;
            DistributedLockRecord distributedLock = null;
            // In a racing condition with no lock record, we might try to insert at the same time as another thread.
            // The transaction does not protect us from INSERTS.
            // To handle this, we will retry to read the record if the INSERT fails.
            for (var i = 0; i < 2; i++)
            {
                var where = $"{nameof(DistributedLockRecord.LockedRecordId)} = @{nameof(DistributedLockRecord.LockedRecordId)}";
                distributedLock = await SearchWhereSingle(where, new { LockedRecordId = recordToLockId}, cancellationToken);
                if (distributedLock != null) break;
                try
                {
                    distributedLock = new DistributedLockRecord
                    {
                        LockId = Guid.NewGuid(),
                        TableName = TableName,
                        LockedRecordId = recordToLockId,
                        ValidUntil = DateTimeOffset.UtcNow + lockTimeSpan.Value
                    };
                    await CreateAsync(distributedLock, cancellationToken);
                    return new Lock<Guid>().From(distributedLock);
                }
                catch (FulcrumConflictException)
                {
                    // Racing condition and we lost, try again to get the lock
                }
            }

            if (distributedLock == null)
            {
                // Our strategy to retry with a read failed, ask the client to retry.
                throw new FulcrumTryAgainException(
                    $"Racing condition for a row lock for record id {recordToLockId} in table {TableName}. Try again.");
            }

            // A lock already existed. This is only a problem if it is still valid and we don't own the lock
            var remainingTime = distributedLock.ValidUntil - DateTimeOffset.UtcNow;
            if (remainingTime > TimeSpan.Zero)
            {
                if (distributedLock.LockId != currentLockId)
                {
                    throw new FulcrumTryAgainException($"The record was already locked.")
                    {
                        RecommendedWaitTimeInSeconds = remainingTime.TotalSeconds
                    };
                }
            }
            if (distributedLock.LockId != currentLockId)
            {
                // The lock was owned by someone else, but it is stale. Steal it
                distributedLock.LockId = Guid.NewGuid();
            }
            distributedLock.ValidUntil = DateTimeOffset.UtcNow + lockTimeSpan.Value;

            try
            {
                await UpdateAsync(distributedLock.Id, distributedLock, cancellationToken);
                return new Lock<Guid>().From(distributedLock);
            }
            catch (FulcrumConflictException)
            {
                throw new FulcrumTryAgainException(
                    $"Racing condition for a row lock for record id {recordToLockId} in table {TableName}. Try again.");
            }

        }

        /// <inheritdoc />
        public Task RemoveAsync(Guid lockedRecordId, Guid lockId, CancellationToken cancellationToken = default)
        {
            var where = $"{nameof(DistributedLockRecord.LockedRecordId)} = @LockedRecordId AND {nameof(DistributedLockRecord.LockId)} = @LockId";
            return DeleteWhereAsync(where, new { LockedRecordId = lockedRecordId, LockId = lockId }, cancellationToken);
        }
    }
}