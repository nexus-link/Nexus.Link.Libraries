using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Nexus.Link.Libraries.Azure.Storage.File;
using Nexus.Link.Libraries.Core.Assert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Storage.Blob
{
    public class AzureBlobContainer : IDirectory
    {
        private readonly CloudBlobContainer _blobContainer;

        internal AzureBlobContainer(CloudBlobContainer container)
        {
            InternalContract.RequireNotNull(container, nameof(container));
            _blobContainer = container;
        }

        public string Name => _blobContainer.Name;

        public Uri Uri => _blobContainer.Uri;

        public async Task<bool> ExistsAsync()
        {
            return await _blobContainer.ExistsAsync();
        }

        public async Task CreateIfNotExistsAsync()
        {
            var requestOptions = new BlobRequestOptions { RetryPolicy = new ExponentialRetry() };
            await _blobContainer.CreateIfNotExistsAsync(requestOptions, null);
        }

        public IFile CreateFile(string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            var blockBlob = _blobContainer.GetBlockBlobReference(name);
            return new AzureBlockBlob(blockBlob);
        }

        public async Task DeleteAsync()
        {
            await _blobContainer.DeleteAsync();
        }

        public async Task<IEnumerable<IDirectoryListItem>> ListContentAsync(CancellationToken ct = default(CancellationToken))
        {
            var blobs = await ListBlobsAsync(ct);
            var items = blobs
            .Select(AzureBlobDirectory.CastToBlob);
            return items;
        }

        private async Task<List<IListBlobItem>> ListBlobsAsync(CancellationToken ct = default(CancellationToken))
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<IListBlobItem>();
            do
            {
                var response = await _blobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            } while (continuationToken != null && !ct.IsCancellationRequested);

            return results;
        }

        public override string ToString() => Name ?? Uri.ToString();

        public string ToLogString() => $"BlobContainer {Name} ({Uri})";
    }
}