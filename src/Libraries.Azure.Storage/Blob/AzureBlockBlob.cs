using Microsoft.WindowsAzure.Storage.Blob;
using Nexus.Link.Libraries.Azure.Storage.File;
using Nexus.Link.Libraries.Core.Assert;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Nexus.Link.Libraries.Azure.Storage.Blob
{
    public class AzureBlockBlob : IFile
    {
        protected CloudBlockBlob CloudBlockBlob { get; }

        internal AzureBlockBlob(CloudBlockBlob blockBlob)
        {
            InternalContract.RequireNotNull(blockBlob, nameof(blockBlob));
            CloudBlockBlob = blockBlob;
        }

        public string Name => CloudBlockBlob.Name;

        public Uri Uri => CloudBlockBlob.Uri;

        public async Task UploadAsync(string content, string contentType = null, CancellationToken cancellationToken = default)
        {
            if (contentType != null) CloudBlockBlob.Properties.ContentType = contentType;
            await CloudBlockBlob.UploadTextAsync(content, Encoding.UTF8, AccessCondition.GenerateEmptyCondition(), null, null, cancellationToken);
        }

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            return await CloudBlockBlob.ExistsAsync(null, null, cancellationToken);
        }

        public async Task UploadAsync(Stream source, CancellationToken cancellationToken = default)
        {
            await CloudBlockBlob.UploadFromStreamAsync(source, AccessCondition.GenerateEmptyCondition(), null, null, cancellationToken);
        }

        public async Task<string> DownloadTextAsync(CancellationToken cancellationToken = default)
        {
            return await CloudBlockBlob.DownloadTextAsync(Encoding.UTF8, AccessCondition.GenerateEmptyCondition(), null,null, cancellationToken);
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            await CloudBlockBlob.DeleteAsync(DeleteSnapshotsOption.None, AccessCondition.GenerateEmptyCondition(), null, null, cancellationToken);
        }

        public override string ToString() => Uri.ToString();

        public string ToLogString() => $"BlobContainer {Name} ({Uri})";
    }
}