using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;

namespace Nexus.Link.Libraries.Azure.Storage.V12.Queue
{
    public class AzureStorageQueue<T> : ICompleteQueue<T>
    {
        public string Name { get; }
        private readonly QueueClientOptions _queueClientOptions;

        private readonly Task<QueueClient> _cloudQueueTask;

        public AzureStorageQueue(string connectionString, string name) : this(connectionString, name, null)
        {
        }

        public AzureStorageQueue(string connectionString, string name, QueueClientOptions queueClientOptions)
        {
            Name = name;
            _queueClientOptions = queueClientOptions;
            _cloudQueueTask = MaybeCreateAndConnectAsync(connectionString, Name, CancellationToken.None);
        }

        public async Task AddMessageAsync(T message, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default)
        {
            // Inspired by https://docs.microsoft.com/en-us/azure/storage/queues/storage-tutorial-queues?tabs=dotnet
            var queue = (await _cloudQueueTask);
            FulcrumAssert.IsNotNull(queue);
            var messageAsString = JsonConvert.SerializeObject(message);
            await queue.SendMessageAsync(messageAsString, timeSpanToWait, TimeSpan.FromSeconds(-1), cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await (await _cloudQueueTask).ClearMessagesAsync(cancellationToken);
        }

        public async Task<T> GetOneMessageNoBlockAsync(CancellationToken cancellationToken = default)
        {
            // Inspired by https://docs.microsoft.com/en-us/azure/storage/queues/storage-tutorial-queues?tabs=dotnet

            var queue = await _cloudQueueTask;
            if (!await queue.ExistsAsync(cancellationToken)) return default;

            var response = await queue.ReceiveMessageAsync(null, cancellationToken);
            var queueMessage = response.Value;
            if (queueMessage == null) return default;
            try
            {
                await queue.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            }
            catch (Exception)
            {
                // Ignore exceptions from delete. This could result in that the same message from the queue is executed more than once.
                // The alternative is to risk losing messages and that is worse.
            }

            var messageAsString = queueMessage.MessageText;
            return messageAsString == null ? default : JsonHelper.SafeDeserializeObject<T>(messageAsString);
        }

        /// <inheritdoc />
        public async Task<T> GetOneMessageNoBlockAsync(Func<T, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            var queue = await _cloudQueueTask;
            if (!await queue.ExistsAsync(cancellationToken)) return default;

            var response = await queue.ReceiveMessageAsync(null, cancellationToken);
            var queueMessage = response.Value;
            if (queueMessage == null) return default;
            var messageAsString = queueMessage.MessageText;
            var item =  messageAsString == null ? default : JsonHelper.SafeDeserializeObject<T>(messageAsString);
            T result;
            try
            {
                result = await action(item, cancellationToken);
            }
            catch (Exception)
            {
                // Release the message
                await queue.UpdateMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt,
                    queueMessage.MessageText, TimeSpan.Zero, cancellationToken);
                throw;
            }

            try
            {
                await queue.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            }
            catch (Exception)
            {
                // This could result in  the same message from the queue is executed more than once.
                // The alternative is to risk losing messages and that is worse.
                // It is up to the action method to deal with this.
            }

            return result;
        }

        public async Task<T> PeekNoBlockAsync(CancellationToken cancellationToken = default)
        {
            var queue = await _cloudQueueTask;
            var response = await queue.PeekMessageAsync(cancellationToken);
            var queueMessage = response.Value;
            var messageAsString = queueMessage?.MessageText;
            return messageAsString == null ? default : JsonHelper.SafeDeserializeObject<T>(messageAsString);
        }

        public async Task<int?> GetApproximateMessageCountAsync(CancellationToken cancellationToken = default)
        {
            var queue = await _cloudQueueTask;
            var properties = await queue.GetPropertiesAsync(cancellationToken);
            return properties.Value.ApproximateMessagesCount;
        }

        public async Task<HealthResponse> GetResourceHealthAsync(CancellationToken cancellationToken = default)
        {
            var queue = await _cloudQueueTask;
            var response = new HealthResponse($"Queue {queue?.Name}");
            return await Task.FromResult(response);
        }

        private async Task<QueueClient> MaybeCreateAndConnectAsync(string connectionString, string name, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            var client = new QueueClient(connectionString, name, _queueClientOptions);
            FulcrumAssert.IsNotNull(client, CodeLocation.AsString(),
                $"Could not create a cloud queue client for queue {name}.");
            
            try
            {
                await client.CreateIfNotExistsAsync(null, cancellationToken);
            }
            catch (Exception e)
            {
                throw new FulcrumResourceException($"Could not create queue '{name}' on storage '{client.Uri}': {e.Message}", e);
            }

            return client;
        }

        // TODO: Remove dependency to IResourceHealth?
        public Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
