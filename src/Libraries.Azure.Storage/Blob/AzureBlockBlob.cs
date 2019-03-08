using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Nexus.Link.Libraries.Azure.Core.File;
using Nexus.Link.Libraries.Core.Assert;

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

        public async Task UploadAsync(string content, string contentType = null)
        {
            if (contentType != null) CloudBlockBlob.Properties.ContentType = contentType;
            await CloudBlockBlob.UploadTextAsync(content);
        }

        public async Task<bool> ExistsAsync()
        {
            return await CloudBlockBlob.ExistsAsync();
        }

        public async Task UploadAsync(Stream source)
        {
            await CloudBlockBlob.UploadFromStreamAsync(source);
        }

        public async Task<string> DownloadTextAsync()
        {
            return await CloudBlockBlob.DownloadTextAsync();
        }


        public async Task DeleteAsync()
        {
            await CloudBlockBlob.DeleteAsync();
        }

        public override string ToString() => Uri.ToString();

        public string ToLogString() => $"BlobContainer {Name} ({Uri})";
    }
}
