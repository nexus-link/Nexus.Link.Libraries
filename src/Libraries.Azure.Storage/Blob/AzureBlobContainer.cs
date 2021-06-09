using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Nexus.Link.Libraries.Azure.Storage.File;
using Nexus.Link.Libraries.Core.Assert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

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

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            return await _blobContainer.ExistsAsync(null, null, cancellationToken);
        }

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default)
        {
            var requestOptions = new BlobRequestOptions { RetryPolicy = new ExponentialRetry() };
            await _blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, requestOptions, null, cancellationToken);
        }

        public IFile CreateFile(string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            var blockBlob = _blobContainer.GetBlockBlobReference(name);
            return new AzureBlockBlob(blockBlob);
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            await _blobContainer.DeleteAsync(AccessCondition.GenerateEmptyCondition(), null, null, cancellationToken);
        }

        public async Task<IEnumerable<IDirectoryListItem>> ListContentAsync(CancellationToken cancellationToken = default)
        {
            var blobs = await ListBlobsAsync(cancellationToken);
            var items = blobs
            .Select(AzureBlobDirectory.CastToBlob);
            return items;
        }

        private async Task<List<IListBlobItem>> ListBlobsAsync(CancellationToken cancellationToken = default)
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<IListBlobItem>();
            do
            {
                var response = await _blobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            } while (continuationToken != null && !cancellationToken.IsCancellationRequested);

            return results;
        }

        public override string ToString() => Name ?? Uri.ToString();

        public string ToLogString() => $"BlobContainer {Name} ({Uri})";
    }
}