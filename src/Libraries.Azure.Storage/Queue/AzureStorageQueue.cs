﻿using System;
using System.Text;
using System.Threading;
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
            _cloudQueueTask = MaybeCreateAndConnectAsync(connectionString, Name, CancellationToken.None);
        }

        public async Task AddMessageAsync(T message, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default)
        {
            var queue = (await _cloudQueueTask);
            FulcrumAssert.IsNotNull(queue);
            var messageAsString = SerializeToString(message);
            await queue.AddMessageAsync(new CloudQueueMessage(messageAsString), null, timeSpanToWait, null, null, cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await (await _cloudQueueTask).ClearAsync(null, null, cancellationToken);
        }

        public async Task<T> GetOneMessageNoBlockAsync(CancellationToken cancellationToken = default)
        {
            var message = await (await _cloudQueueTask).GetMessageAsync(null, null, null, cancellationToken);
            if (message == null) return default;
            var response = FromByteArray(message.AsBytes);
            await (await _cloudQueueTask).DeleteMessageAsync(message.Id, message.PopReceipt, null, null, cancellationToken);
            return response;
        }

        public async Task<T> PeekNoBlockAsync(CancellationToken cancellationToken = default)
        {
            var message = await (await _cloudQueueTask).PeekMessageAsync(null, null, cancellationToken);
            var response = message == null ? default : FromByteArray(message.AsBytes);
            return response;
        }

        public async Task<int?> GetApproximateMessageCountAsync(CancellationToken cancellationToken = default)
        {
            var cloudQueue = await _cloudQueueTask; 
            await cloudQueue.FetchAttributesAsync(null, null, cancellationToken);
            var count = cloudQueue.ApproximateMessageCount;
            return count;
        }

        public async Task<HealthResponse> GetResourceHealthAsync(CancellationToken cancellationToken = default)
        {
            var queue = await _cloudQueueTask;
            var response = new HealthResponse($"Queue {queue?.Name}");
            return await Task.FromResult(response);
        }

        private async Task<CloudQueue> MaybeCreateAndConnectAsync(string connectionString, string name, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var client = storageAccount.CreateCloudQueueClient();
            FulcrumAssert.IsNotNull(client, null, "Could not create a cloud queue client.");
            FulcrumAssert.IsNotNull(client, null, $"Expected to have a queue client ready for queue {name}.");
            var queue = client.GetQueueReference(name);
            FulcrumAssert.IsNotNull(queue, null, $"Failed to create a queue reference to {name}");
            await queue.CreateIfNotExistsAsync(null, null, cancellationToken);
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
        public Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
