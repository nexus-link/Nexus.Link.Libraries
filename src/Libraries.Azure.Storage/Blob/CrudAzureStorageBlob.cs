using Newtonsoft.Json;
using Nexus.Link.Libraries.Azure.Storage.File;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Azure.Storage.Blob
{
    public class CrudAzureStorageBlob<TItem, TId> :
        CrudAzureStorageBlob<TItem, TItem, TId>,
        ICrud<TItem, TId>
    {
        public CrudAzureStorageBlob(string connectionString, string containerName)
        : base(connectionString, containerName)
        {
        }
    }

    public class CrudAzureStorageBlob<TModelCreate, TModel, TId> :
        ICrud<TModelCreate, TModel, TId>
        where TModel : TModelCreate
    {
        public CrudAzureStorageBlob(string connectionString, string containerName)
        {
            var client = new AzureBlobClient(connectionString);
            Directory = client.GetBlobContainer(containerName);
        }

        public IDirectory Directory { get; }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TModelCreate item, CancellationToken token = new CancellationToken())
        {
            var id = StorageHelper.CreateNewId<TId>();
            await CreateWithSpecifiedIdAsync(id, item, token);
            return id;
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = new CancellationToken())
        {
            var id = await CreateAsync(item, token);
            return await ReadAsync(id, token);
        }

        public async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var createTask = Directory.CreateIfNotExistsAsync();
            var fileName = CreateFileName(id);
            await createTask;
            var file = Directory.CreateFile(fileName);
            var fileExistsTask = file.ExistsAsync();
            var dbItem = StorageHelper.DeepCopy<TModel, TModelCreate>(item);
            StorageHelper.MaybeCreateNewEtag(dbItem);
            StorageHelper.MaybeUpdateTimeStamps(dbItem, true);
            StorageHelper.MaybeSetId(id, dbItem);
            StorageHelper.MaybeValidate(dbItem);
            var content = JsonConvert.SerializeObject(dbItem);
            FulcrumAssert.IsTrue(!await fileExistsTask, null, $"File ({fileName}) unexpectedly already existed in directory {Directory.Name}");
            await file.UploadAsync(content, "application/json");
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item, CancellationToken token = new CancellationToken())
        {
            await CreateWithSpecifiedIdAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = new CancellationToken())
        {
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId itemId, CancellationToken token = new CancellationToken())
        {
            throw new FulcrumNotImplementedException();
        }

        public async Task<TModel> ReadAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            if (!await Directory.ExistsAsync()) return default(TModel);
            var fileName = CreateFileName(id);
            var file = Directory.CreateFile(fileName);
            if (!await file.ExistsAsync()) return default(TModel);
            var content = await file.DownloadTextAsync();
            var dbItem = JsonHelper.SafeDeserializeObject<TModel>(content);
            FulcrumAssert.IsNotNull(dbItem, CodeLocation.AsString());
            FulcrumAssert.IsValidated(dbItem, CodeLocation.AsString());
            return dbItem;
        }

        public async Task DeleteAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            if (!await Directory.ExistsAsync()) return;
            var fileName = CreateFileName(id);
            var file = Directory.CreateFile(fileName);
            if (!await file.ExistsAsync()) return;
            await file.DeleteAsync();
        }

        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            limit = limit ?? int.MaxValue;
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            // TODO: Make this smarter. Currently it always gets the entire list of directory content. Can we just get up to the limit?
            var list = new List<TModel>();
            var skipped = 0;
            var found = 0;
            var directoryItems = await Directory.ListContentAsync();
            var directoryItemList = directoryItems as IList<IDirectoryListItem> ?? directoryItems.ToList();
            foreach (var item in directoryItemList)
            {
                if (skipped < offset)
                {
                    skipped++;
                    continue;
                }
                if (found >= limit.Value) break;
                var file = item as IFile;
                FulcrumAssert.IsNotNull(file, $"Expected {item.ToLogString()} to be a file.");
                if (file == null) continue;
                var content = await file.DownloadTextAsync();
                var o = JsonHelper.SafeDeserializeObject<TModel>(content);
                list.Add(o);
                found++;
            }
            return new PageEnvelope<TModel>(offset, limit.Value, directoryItemList.Count, list);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TModel>> ReadAllAsync(int limit = 2147483647, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return StorageHelper.ReadPagesAsync<TModel>((offset, ct) => ReadAllWithPagingAsync(offset, null, ct), limit, token);
        }

        public async Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            if (!await Directory.ExistsAsync()) return;
            await Directory.DeleteAsync();
        }

        private static string CreateFileName(TId itemId) => $"{itemId}.json";

        public async Task UpdateAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (!await Directory.ExistsAsync()) throw new FulcrumNotFoundException($"Could not find the item with id {id}.");
            var verifyEtagTask = MaybeVerifyEtagForUpdateAsync(id, item, token);
            var fileName = CreateFileName(id);
            var file = Directory.CreateFile(fileName);
            await verifyEtagTask;
            StorageHelper.MaybeCreateNewEtag(item);
            StorageHelper.MaybeSetId(id, item);
            StorageHelper.MaybeUpdateTimeStamps(item, false);
            var content = JsonConvert.SerializeObject(item);
            if (!await file.ExistsAsync()) throw new FulcrumNotFoundException($"Could not find the item with id {id}.");
            await file.UploadAsync(content, "application/json");
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = new CancellationToken())
        {
            await UpdateAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IOptimisticConcurrencyControlByETag"/>
        /// then the old value is read using <see cref="ReadAsync"/> and the values are verified to be equal.
        /// The Etag of the item is then set to a new value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        protected virtual async Task MaybeVerifyEtagForUpdateAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            if (item is IOptimisticConcurrencyControlByETag etaggable)
            {
                var oldItem = await ReadAsync(id, token);
                if (oldItem != null)
                {
                    var oldEtag = (oldItem as IOptimisticConcurrencyControlByETag)?.Etag;
                    if (oldEtag?.ToLowerInvariant() != etaggable.Etag?.ToLowerInvariant())
                        throw new FulcrumConflictException($"The updated item ({item}) had an old ETag value.");
                }
            }
        }
    }
}