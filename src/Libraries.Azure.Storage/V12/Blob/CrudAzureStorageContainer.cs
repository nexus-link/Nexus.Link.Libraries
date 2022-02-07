using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using PageInfo = Nexus.Link.Libraries.Core.Storage.Model.PageInfo;

namespace Nexus.Link.Libraries.Azure.Storage.V12.Blob
{
    /// <inheritdoc cref="CrudAzureStorageContainer{TCreateModel, TModel}" />
    public class CrudAzureStorageContainer<TModel> : CrudAzureStorageContainer<TModel, TModel>, ICrud<TModel, string>
    {
        public CrudAzureStorageContainer(BlobContainerClient client) : base(client)
        {
        }

        public CrudAzureStorageContainer(string connectionString, string containerName) : base(connectionString,
            containerName)
        {
        }
    }

    /// <inheritdoc />
    public class CrudAzureStorageContainer<TModelCreate, TModel> : ICrud<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        public BlobContainerClient Client { get; }
        private readonly CrudConvenience<TModelCreate, TModel, string> _convenience;

        public CrudAzureStorageContainer(BlobContainerClient client)
        {
            client.CreateIfNotExists();
            Client = client;
            _convenience = new CrudConvenience<TModelCreate, TModel, string>(this);
        }

        public CrudAzureStorageContainer(string connectionString, string containerName) : this(
            new BlobContainerClient(connectionString, containerName))
        {
        }

        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var id = await CreateAsync(item, cancellationToken);
            return await ReadAsync(id, cancellationToken);
        }

        public async Task CreateWithSpecifiedIdAsync(string id, TModelCreate item,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var blob = Client.GetBlobClient(id);
            if (await blob.ExistsAsync(cancellationToken))
            {
                throw new FulcrumConflictException($"Blob {id} already exists");
            }

            await CreateOrUpdateAsync(id, item, cancellationToken);
        }

        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string id, TModelCreate item,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            await CreateWithSpecifiedIdAsync(id, item, cancellationToken);
            return await ReadAsync(id, cancellationToken);
        }

        public Task<IEnumerable<TModel>> ReadAllAsync(int limit = Int32.MaxValue,
            CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync<TModel>((o, ct) => ReadAllWithPagingAsync(o, null, ct), limit,
                cancellationToken);
        }

        public async Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset,
            int? limit = null, CancellationToken cancellationToken = default)
        {
            limit ??= PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            var allItems = (await ReadAllAsync(int.MaxValue, cancellationToken))
                .ToList();

            var list = SearchHelper.FilterAndSort(allItems, details)
                .Skip(offset)
                .Take(limit.Value);
            var page = new PageEnvelope<TModel>(offset, limit.Value, allItems.Count(), list);
            return page;
        }

        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        public async Task<TModel> UpdateAndReturnAsync(string id, TModel item,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            await UpdateAsync(id, item, cancellationToken);
            return await ReadAsync(id, cancellationToken);
        }

        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public Task<Lock<string>> ClaimLockAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public Task ReleaseLockAsync(string id, string lockId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Lock<string>> ClaimDistributedLockAsync(string id, TimeSpan? lockTimeSpan = null,
            string currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            if (lockTimeSpan.HasValue)
            {
                var seconds = lockTimeSpan.Value.TotalSeconds;
                if (seconds != -1.0)
                {
                    InternalContract.RequireGreaterThanOrEqualTo(15, seconds, nameof(lockTimeSpan));
                    InternalContract.RequireLessThanOrEqualTo(60, seconds, nameof(lockTimeSpan));
                }
            }
            else
            {
                lockTimeSpan = TimeSpan.FromSeconds(30);
            }

            var blob = Client.GetBlobClient(id);
            var lease = blob.GetBlobLeaseClient();
            var response = await lease.AcquireAsync(lockTimeSpan.Value, null, cancellationToken);
            if (response == null)
            {
                throw new FulcrumTryAgainException($"The blob {id} was already locked.");
            }

            return new Lock<string>
            {
                ItemId = id,
                LockId = response.Value.LeaseId,
                ValidUntil = DateTimeOffset.UtcNow + lockTimeSpan.Value
            };

        }

        public async Task ReleaseDistributedLockAsync(string id, string lockId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));

            var blob = Client.GetBlobClient(id);
            var lease = blob.GetBlobLeaseClient(lockId);
            await lease.ReleaseAsync(cancellationToken: cancellationToken);
        }

        public Task ClaimTransactionLockAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TModel> ClaimTransactionLockAndReadAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateAsync(TModelCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var id = Guid.NewGuid().ToGuidString();
            await CreateWithSpecifiedIdAsync(id, item, cancellationToken);
            return id;
        }

        public async Task<TModel> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var blob = Client.GetBlobClient(id);
            if (!await blob.ExistsAsync(cancellationToken)) return default;
            var response = await blob.DownloadContentAsync(cancellationToken);
            FulcrumAssert.IsNotNull(response, CodeLocation.AsString());
            var contentAsString = response.Value.Content.ToString();
            return JsonConvert.DeserializeObject<TModel>(contentAsString);
        }

        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit.HasValue)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }

            // TODO: Make this smarter. Currently it always gets the entire list of directory content. Can we just get up to the limit?
            var taskList = new List<Task<TModel>>();
            var skipped = 0;
            var found = 0;
            var pages = Client.GetBlobsAsync().AsPages(null, limit);
            await foreach (var page in pages.WithCancellation(cancellationToken))
            {
                foreach (var blob in page.Values)
                {
                    if (skipped < offset)
                    {
                        skipped++;
                        continue;
                    }

                    if (limit.HasValue && found >= limit.Value) break;
                    var task = ReadAsync(blob.Name, cancellationToken);
                    taskList.Add(task);
                    found++;
                }
            }

            var itemList = new List<TModel>();
            foreach (var task in taskList)
            {
                itemList.Add(await task);
            }

            return new PageEnvelope<TModel>(offset, limit ?? itemList.Count, itemList.Count, itemList);
        }

        public async Task UpdateAsync(string id, TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var oldItem = await ReadAsync(id, cancellationToken);
            if (Equals(oldItem, default))
            {
                throw new FulcrumNotFoundException($"Could not find blob {id}.");
            }

            await item.ValidateEtagAsync(id, this, cancellationToken);

            await DeleteAsync(id, cancellationToken);
            await CreateOrUpdateAsync(id, item, cancellationToken);
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var blob = Client.GetBlobClient(id);
            return blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots,
                cancellationToken: cancellationToken);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            var resultSegment = Client.GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, null)
                .AsPages(default, null);
            var taskList = new List<Task>();

            foreach (var blobPage in resultSegment)
            {
                foreach (var blob in blobPage.Values)
                {
                    if (!blob.IsBlob) continue;
                    var task = Client.DeleteBlobIfExistsAsync(blob.Blob.Name, cancellationToken: cancellationToken);
                    taskList.Add(task);
                }
            }

            await Task.WhenAll(taskList);
        }

        private async Task CreateOrUpdateAsync(string id, TModelCreate item,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var finalItem = StorageHelper.DeepCopy<TModel, TModelCreate>(item);
            StorageHelper.MaybeCreateNewEtag(finalItem);
            StorageHelper.MaybeUpdateTimeStamps(finalItem, true);
            StorageHelper.MaybeSetId(id, finalItem);
            InternalContract.RequireValidated(finalItem, nameof(item));
            var blob = Client.GetBlobClient(id);
            await blob.UploadAsync(BinaryData.FromString(JsonConvert.SerializeObject(finalItem)),
                cancellationToken);
        }
    }
}