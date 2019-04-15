using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{
    public class CrudLockTable
    {
        private CrudSql<Lock<Guid>> _storage;

        public CrudLockTable(string connectionString)
        {
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "CrudLock",
                CreatedAtColumnName = "RecordCreatedAt",
                UpdatedAtColumnName = "RecordUpdatedAt",
                CustomColumnNames = new[] {"ObjectId", "ValidUntil"},
                OrderBy = new string[] { }
            };

            _storage = new CrudSql<Lock<Guid>>(connectionString, tableMetadata);
        }

        public async Task AddLockAsync(Guid itemId, TimeSpan lockSpan,
            CancellationToken token = new CancellationToken())
        {
            while (true)
            {
                try
                {
                    var newLock = new Lock<Guid>
                    {
                        ItemId = itemId,
                        ValidUntil = DateTimeOffset.Now.Add(lockSpan)
                    };
                    await _storage.CreateAsync(newLock, token);
                    return;
                }
                catch (SqlException e) when (e.Number == 2601) // Duplicate key
                {
                    var currentLock = await GetLockAsync(itemId, token);
                    if (currentLock == null) continue;
                    var remainingTime = currentLock.ValidUntil.Subtract(DateTimeOffset.Now);
                    if (remainingTime > TimeSpan.Zero)
                    {
                        var message =
                            $"Item {itemId} is locked by someone else. The lock will be released before {currentLock.ValidUntil}";
                        var exception = new FulcrumTryAgainException(message)
                        {
                            RecommendedWaitTimeInSeconds = remainingTime.Seconds
                        };
                        throw exception;
                    }

                    await DeleteOldLockAsync(itemId, token);
                }
            }
        }

        public async Task<Lock<Guid>> GetLockAsync(Guid itemId, CancellationToken token = new CancellationToken())
        {
            return await _storage.SearchFirstWhereAsync($"ItemId=@ItemId", null, new {ItemId = itemId}, token);
        }

        public Task DeleteAsync(Guid lockId, Guid itemId, TimeSpan lockSpan,
            CancellationToken token = new CancellationToken())
        {
            return _storage.ExecuteAsync($"DELETE FROM [{_storage.TableName}] WHERE Id=@lockId AND ItemId=@ItemId",
                new {Id = lockId, ItemId = itemId}, token);
        }

        public Task DeleteAllOldLocksAsync(CancellationToken token = new CancellationToken())
        {
            return _storage.ExecuteAsync($"DELETE FROM [{_storage.TableName}] WHERE ValidUntil < @Now",
                new { DateTimeOffset.Now }, token);
        }

        public Task DeleteOldLockAsync(Guid itemId, CancellationToken token = new CancellationToken())
        {
            return _storage.ExecuteAsync($"DELETE FROM [{_storage.TableName}] WHERE ItemId=@ItemId AND ValidUntil < @Now",
                new { ItemId = itemId, Now=DateTimeOffset.Now }, token);
        }
    }
}
