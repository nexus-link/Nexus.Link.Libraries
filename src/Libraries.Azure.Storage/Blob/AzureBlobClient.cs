using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Azure.Storage.Blob
{
    public class AzureBlobClient
    {
        private static readonly string Namespace = typeof(AzureBlobClient).Namespace;
        private readonly CloudBlobClient _blobClient;

        public AzureBlobClient(string connectionString)
        {
            InternalContract.RequireNotNullOrWhiteSpace(connectionString, nameof(connectionString));
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
            FulcrumAssert.IsNotNull(_blobClient, null, "Could not create a cloud blob client.");
        }

        public AzureBlobContainer GetBlobContainer(string name)
        {
            var container = _blobClient.GetContainerReference(name);
            return new AzureBlobContainer(container);
        }

    }
}
