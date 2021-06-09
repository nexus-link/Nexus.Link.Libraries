using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Table
{
    /// <summary>
    /// General class for storing objects in an Azure Storage Table.
    /// </summary>
    public class AzureStorageTable<TStorableItemCreate, TStorableItem>
        where TStorableItem : TStorableItemCreate, IOptimisticConcurrencyControlByETag
    {
        public CloudTable Table { get; }

        public AzureStorageTable(string connectionString, string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(connectionString, nameof(connectionString));
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var client = storageAccount.CreateCloudTableClient();
            FulcrumAssert.IsNotNull(client, null, "Could not create a cloud table client.");
            Table = client.GetTableReference(name);
        }

        public async Task CreateAsync(string partitionKey, string rowKey, TStorableItemCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            InternalContract.RequireNotNullOrWhiteSpace(rowKey, nameof(rowKey));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var dbItem = StorageHelper.DeepCopy<TStorableItem, TStorableItemCreate>(item);
            StorageHelper.MaybeCreateNewEtag(dbItem);
            StorageHelper.MaybeUpdateTimeStamps(dbItem, true);
            InternalContract.RequireValidated(dbItem, nameof(item));

            var tableRequestOptions = new TableRequestOptions();
            var operationContext = new OperationContext();
            var createTableTask = Table.CreateIfNotExistsAsync(tableRequestOptions, operationContext, token);
            var entity = new JsonEntity(partitionKey, rowKey)
            {
                ETag = dbItem.Etag,
                JsonAsString = JsonConvert.SerializeObject(dbItem)
            };
            await createTableTask;
            await Table.ExecuteAsync(TableOperation.Insert(entity, true), tableRequestOptions, operationContext, token);
        }

        public async Task<TStorableItem> ReadAsync(string partitionKey, string rowKey, CancellationToken token = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            InternalContract.RequireNotNullOrWhiteSpace(rowKey, nameof(rowKey));

            var tableRequestOptions = new TableRequestOptions();
            var operationContext = new OperationContext();
            if (!await Table.ExistsAsync(tableRequestOptions, operationContext, token)) return default;
            var result = await Table.ExecuteAsync(TableOperation.Retrieve<JsonEntity>(partitionKey, rowKey), tableRequestOptions, operationContext, token);
            if (!(result.Result is JsonEntity entity)) return default;
            var item = JsonHelper.SafeDeserializeObject<TStorableItem>(entity.JsonAsString);
            item.Etag = entity.ETag;
            return item;
        }

        public async Task<TStorableItem> UpdateAsync(string partitionKey, string rowKey, TStorableItem item, CancellationToken token = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            InternalContract.RequireNotNullOrWhiteSpace(rowKey, nameof(rowKey));
            InternalContract.RequireNotNull(item, nameof(item));

            var tableRequestOptions = new TableRequestOptions();
            var operationContext = new OperationContext();
            FulcrumAssert.IsTrue(await Table.ExistsAsync(tableRequestOptions, operationContext, token), null, $"Expected {Table.Name} to exist.");

            var entity = new JsonEntity(partitionKey, rowKey)
            {
                ETag = item.Etag,
                JsonAsString = JsonConvert.SerializeObject(item)
            };
            await Table.ExecuteAsync(TableOperation.Replace(entity), tableRequestOptions, operationContext, token);
            item.Etag = entity.ETag;
            return item;
        }

        public async Task DeleteAsync(string partitionKey, string rowKey, CancellationToken token = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            InternalContract.RequireNotNullOrWhiteSpace(rowKey, nameof(rowKey));

            var tableRequestOptions = new TableRequestOptions();
            var operationContext = new OperationContext();
            if (!await Table.ExistsAsync(tableRequestOptions, operationContext, token)) return;

            var entity = new JsonEntity(partitionKey, rowKey) { ETag = "*" };
            await Table.ExecuteAsync(TableOperation.Delete(entity), tableRequestOptions, operationContext, token);
        }

        public async Task DeleteTableAsync(CancellationToken cancellationToken = default)
        {
            await Table.DeleteAsync(null, null, cancellationToken);
        }

        public async Task DeleteItemsAsync(CancellationToken token = default)
        {
            var enumerator = new PageEnvelopeEnumeratorAsync<JsonEntity>((offset, ct) => ReadAllJsonEntitiesWithPagingAsync(offset, 50, ct), token);
            await DeleteItemsAsync(enumerator, token);
        }

        public async Task DeleteItemsAsync(string partitionKey, CancellationToken token = default)
        {
            var enumerator = new PageEnvelopeEnumeratorAsync<JsonEntity>((offset, ct) => ReadAllJsonEntitiesWithPagingAsync(partitionKey, offset, 50, ct), token);
            await DeleteItemsAsync(enumerator, token);
        }

        private async Task DeleteItemsAsync(PageEnvelopeEnumeratorAsync<JsonEntity> enumerator, CancellationToken cancellationToken)
        {
            var taskList = new List<Task>();
            while (await enumerator.MoveNextAsync())
            {
                var item = enumerator.Current;
                var task = DeleteAsync(item.PartitionKey, item.RowKey, cancellationToken);
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
        }

        public async Task<PageEnvelope<TStorableItem>> ReadAllWithPagingAsync(int offset = 0, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            limit = limit ?? int.MaxValue;
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            var page = await ReadAllJsonEntitiesWithPagingAsync(offset, limit.Value, token);
            var list = page.Data.Select(ToStorableItem);
            return new PageEnvelope<TStorableItem>
            {
                PageInfo = page.PageInfo,
                Data = list
            };
        }

        public async Task<PageEnvelope<TStorableItem>> ReadAllWithPagingAsync(string partitionKey, int offset = 0, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            limit = limit ?? int.MaxValue;
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            var page = await ReadAllJsonEntitiesWithPagingAsync(partitionKey, offset, limit.Value, token);
            var list = page.Data.Select(ToStorableItem);
            return new PageEnvelope<TStorableItem>
            {
                PageInfo = page.PageInfo,
                Data = list
            };
        }

        private TStorableItem ToStorableItem(JsonEntity jsonEntity)
        {
            var o = JsonHelper.SafeDeserializeObject<TStorableItem>(jsonEntity.JsonAsString);
            o.Etag = jsonEntity.ETag;
            return o;
        }

        private async Task<PageEnvelope<JsonEntity>> ReadAllJsonEntitiesWithPagingAsync(int offset, int limit, CancellationToken ct = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            // TODO: Make this smarter. Currently it always gets the entire list of directory content. Can we just get up to the limit?
            var query = new TableQuery<JsonEntity>();
            var jsonEntities = await ExecuteQueryAsync(query, ct);
            return await CreatePageEnvelopeAsync(offset, limit, jsonEntities);
        }

        private async Task<PageEnvelope<JsonEntity>> ReadAllJsonEntitiesWithPagingAsync(string partitionKey, int offset, int limit, CancellationToken ct = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            // TODO: Make this smarter. Currently it always gets the entire list of directory content. Can we just get up to the limit?
            var query = new TableQuery<JsonEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var jsonEntities = await ExecuteQueryAsync(query, ct);
            return await CreatePageEnvelopeAsync(offset, limit, jsonEntities);
        }
        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> query, CancellationToken ct = default) where T : ITableEntity, new()
        {
            var runningQuery = new TableQuery<T>()
            {
                FilterString = query.FilterString,
                SelectColumns = query.SelectColumns
            };

            var items = new List<T>();
            TableContinuationToken token = null;

            do
            {
                runningQuery.TakeCount = query.TakeCount - items.Count;

                var seg = await Table.ExecuteQuerySegmentedAsync<T>(runningQuery, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);

            } while (token != null && !ct.IsCancellationRequested && (query.TakeCount == null || items.Count < query.TakeCount.Value));

            return items;
        }

        private static async Task<PageEnvelope<T>> CreatePageEnvelopeAsync<T>(int offset, int limit, IEnumerable<T> items)
        {
            var itemList = items as IList<T> ?? items.ToList();
            var list = new List<T>();
            var skipped = 0;
            var found = 0;
            foreach (var item in itemList)
            {
                if (skipped < offset)
                {
                    skipped++;
                    continue;
                }
                if (found >= limit) break;
                list.Add(item);
                found++;
            }
            return await Task.FromResult(new PageEnvelope<T>(offset, limit, itemList.Count, list));
        }
    }

    internal class JsonEntity : TableEntity
    {
        public JsonEntity() { }

        public
            JsonEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string JsonAsString { get; set; }
    }
}
