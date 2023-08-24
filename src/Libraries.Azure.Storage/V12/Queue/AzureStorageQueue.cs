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

        private Exception _lastWarningException;
        private int _warningExceptions;
        private Exception _lastErrorException;
        private int _errorsExceptions;

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
            catch (Exception ex)
            {
                _warningExceptions++;
                _lastWarningException = ex;
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
            var item = messageAsString == null ? default : JsonHelper.SafeDeserializeObject<T>(messageAsString);
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
            catch (Exception ex)
            {
                _warningExceptions++;
                _lastWarningException = ex;
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
            catch (Exception ex)
            {
                _errorsExceptions++;
                _lastErrorException = ex;
                throw new FulcrumResourceException($"Could not create queue '{name}' on storage '{client.Uri}': {ex.Message}", ex);
            }

            return client;
        }

        /// <inheritdoc />
        public Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var healthResponse = new HealthResponse("Azure Storage Queue")
            {
                Status = HealthResponse.StatusEnum.Ok
            };
            if (_lastErrorException != null)
            {
                healthResponse.Status = HealthResponse.StatusEnum.Error;
                healthResponse.Message =
                    $"Error exceptions: {_errorsExceptions}. Last error exception: {_lastErrorException}\rWarnings: {_warningExceptions}. Last warning exception: {_lastWarningException}";
            }
            else if (_lastWarningException != null)
            {
                healthResponse.Status = HealthResponse.StatusEnum.Warning;
                healthResponse.Message =
                    $"Warning exceptions: {_warningExceptions}. Last warning exception: {_lastWarningException}";
            }
            return Task.FromResult(healthResponse);
        }

        /// <inheritdoc />
        public Task<HealthInfo> GetResourceHealth2Async(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var healthInfo = new HealthInfo("Azure Storage Queue")
            {
                Status = HealthInfo.StatusEnum.Ok
            };
            if (_lastErrorException != null)
            {
                healthInfo.Status = HealthInfo.StatusEnum.Error;
                healthInfo.Message =
                    $"Error exceptions: {_errorsExceptions}. Last error exception: {_lastErrorException}\rWarnings: {_warningExceptions}. Last warning exception: {_lastWarningException}";
            }
            else if (_lastWarningException != null)
            {
                healthInfo.Status = HealthInfo.StatusEnum.Warning;
                healthInfo.Message =
                    $"Warning exceptions: {_warningExceptions}. Last warning exception: {_lastWarningException}";
            }
            return Task.FromResult(healthInfo);
        }
    }
}
