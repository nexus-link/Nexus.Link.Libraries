using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Queue
{
    public class AzureStorageQueue<T> : ICompleteQueue<T>
    {
        public string Name { get; }

        private readonly Task<CloudQueue> _cloudQueueTask;

        public AzureStorageQueue(string connectionString, string name)
        {
            Name = name;
            _cloudQueueTask = MaybeCreateAndConnectAsync(connectionString, Name);
        }

        public async Task AddMessageAsync(T message, TimeSpan? timeSpanToWait = null)
        {
            var queue = (await _cloudQueueTask);
            FulcrumAssert.IsNotNull(queue);
            var messageAsString = SerializeToString(message);
            await queue.AddMessageAsync(new CloudQueueMessage(messageAsString), null, timeSpanToWait, null, null);
        }

        public async Task ClearAsync()
        {
            await (await _cloudQueueTask).ClearAsync();
        }

        public async Task<T> GetOneMessageNoBlockAsync()
        {
            var message = await (await _cloudQueueTask).GetMessageAsync();
            if (message == null) return default(T);
            var response = FromByteArray(message.AsBytes);
            await (await _cloudQueueTask).DeleteMessageAsync(message.Id, message.PopReceipt);
            return response;
        }

        public async Task<T> PeekNoBlockAsync()
        {
            var message = await (await _cloudQueueTask).PeekMessageAsync();
            var response = message == null ? default(T) : FromByteArray(message.AsBytes);
            return response;
        }

        public async Task<int?> GetApproximateMessageCountAsync()
        {
            var cloudQueue = await _cloudQueueTask; 
            await cloudQueue.FetchAttributesAsync();
            var count = cloudQueue.ApproximateMessageCount;
            return count;
        }

        public async Task<HealthResponse> GetResourceHealthAsync()
        {
            var queue = await _cloudQueueTask;
            var response = new HealthResponse($"Queue {queue?.Name}");
            return await Task.FromResult(response);
        }

        private async Task<CloudQueue> MaybeCreateAndConnectAsync(string connectionString, string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var client = storageAccount.CreateCloudQueueClient();
            FulcrumAssert.IsNotNull(client, null, "Could not create a cloud queue client.");
            FulcrumAssert.IsNotNull(client, null, $"Expected to have a queue client ready for queue {name}.");
            var queue = client.GetQueueReference(name);
            FulcrumAssert.IsNotNull(queue, null, $"Failed to create a queue reference to {name}");
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        private string SerializeToString(T message)
        {
            return JsonConvert.SerializeObject(message);
        }

        private T FromByteArray(byte[] byteArray)
        {
            var messageAsString = Encoding.UTF8.GetString(byteArray);
            return JsonHelper.SafeDeserializeObject<T>(messageAsString);
        }

        // TODO: Remove dependency to IResourceHealth?
        public Task<HealthResponse> GetResourceHealthAsync(Tenant tenant)
        {
            throw new NotImplementedException();
        }
    }
}
