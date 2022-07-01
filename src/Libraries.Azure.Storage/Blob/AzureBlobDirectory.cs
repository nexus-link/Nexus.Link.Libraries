using Microsoft.WindowsAzure.Storage.Blob;
using Nexus.Link.Libraries.Azure.Storage.File;
using Nexus.Link.Libraries.Core.Error.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Storage.Blob
{
    public class AzureBlobDirectory : IDirectory
    {
        private readonly CloudBlobDirectory _blobDirectory;

        internal AzureBlobDirectory(CloudBlobDirectory directory)
        {
            _blobDirectory = directory;
            Container = new AzureBlobContainer(directory.Container);
        }

        public string Name => _blobDirectory.Container.Name;

        public Uri Uri => _blobDirectory.Uri;

        public AzureBlobContainer Container { get; }

        public IFile CreateFile(string name)
        {
            var blockBlob = _blobDirectory.GetBlockBlobReference(name);
            return new AzureBlockBlob(blockBlob);
        }

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            return await Container.ExistsAsync(cancellationToken);
        }

        public Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default)
        {
            throw new FulcrumNotImplementedException();
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            throw new FulcrumNotImplementedException();
        }

        public async Task<IEnumerable<IDirectoryListItem>> ListContentAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult((await ListBlobsAsync(cancellationToken))
                .Select(CastToBlob));
        }

        private async Task<List<IListBlobItem>> ListBlobsAsync(CancellationToken ct = default)
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<IListBlobItem>();
            do
            {
                var response = await _blobDirectory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            } while (continuationToken != null && !ct.IsCancellationRequested);

            return results;
        }

        public override string ToString() => Uri.ToString();

        public string ToLogString() => $"BlobContainer {Name} ({Uri})";

        internal static IDirectoryListItem CastToBlob(IListBlobItem listItem)
        {
            var type = listItem.GetType();
            if (type == typeof(CloudBlobDirectory)) return new AzureBlobDirectory((CloudBlobDirectory)listItem);
            if (type == typeof(CloudBlockBlob)) return new AzureBlockBlob((CloudBlockBlob)listItem);
            throw new FulcrumNotImplementedException($"Does not support type {type} yet.");
        }
    }
}