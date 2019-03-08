using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;
using Nexus.Link.Libraries.Core.Threads;

// ReSharper disable RedundantExtendsListEntry

namespace Nexus.Link.Libraries.Core.Queue.Logic
{
    internal class MessageEnvelope<T>
    {
        public MessageEnvelope(T message, TimeSpan? timeSpanToWait)
        {
            Id = Guid.NewGuid();
            Message = message;
            CreatedAt = DateTimeOffset.Now;
            PostponeUntil = timeSpanToWait == null
                ? (DateTimeOffset?) null
                : DateTimeOffset.Now.Add(timeSpanToWait.Value);
        }

        public Guid Id { get; set; }
        public T Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PostponeUntil { get; set; }

        public bool IsActivationTime => PostponeUntil == null || DateTimeOffset.Now > PostponeUntil;

        public TimeSpan ActiveTimeInQueue => PostponeUntil == null
            ? DateTimeOffset.Now.Subtract(CreatedAt)
            : DateTimeOffset.Now.Subtract(PostponeUntil.Value);
    }

    /// <summary>
    ///     A generic interface for adding strings to a queue.
    /// </summary>
    public partial class MemoryQueue<T> : IBaseQueue, ICompleteQueue<T>
    {
        /// <summary>
        ///     Delegate for the action to take on every queue item.
        ///     The delegate must never fail.
        /// </summary>
        /// <param name="item">A queue item.</param>
        public delegate Task QueueItemActionDelegate(T item);

        private readonly bool _actionsCanExecuteWithoutIndividualAwait;
        private readonly ConcurrentQueue<MessageEnvelope<T>> _queue;
        private readonly QueueItemActionDelegate _queueItemAction;
        private Thread _backgroundWorkerThread;

        /// <summary>
        ///     Constructor
        /// </summary>
        public MemoryQueue(string name) : this(name, null, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MemoryQueue(string name, QueueItemActionDelegate queueItemAction) : this(name, queueItemAction, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The name of the queue</param>
        /// <param name="queueItemAction">Optional action that will be called for every item taken from the queue.</param>
        /// <param name="actionsCanExecuteWithoutIndividualAwait">
        ///     True means that the many calls can be made to the
        ///     <paramref name="queueItemAction" /> action without awaiting every single one.
        /// </param>
        /// <remarks>
        ///     If <paramref name="actionsCanExecuteWithoutIndividualAwait" /> is true, then you guarantee that it is possible to
        ///     run many <paramref name="queueItemAction" /> "in parallel" without interference.
        /// </remarks>
        public MemoryQueue(string name, QueueItemActionDelegate queueItemAction = null,
            bool actionsCanExecuteWithoutIndividualAwait = false)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            Name = name;
            _queue = new ConcurrentQueue<MessageEnvelope<T>>();
            _queueItemAction = queueItemAction;
            _actionsCanExecuteWithoutIndividualAwait = actionsCanExecuteWithoutIndividualAwait;
        }

        /// <summary>
        ///     This is a property specifically for unit testing.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool OnlyForUnitTest_HasAliveBackgroundWorker
        {
            get
            {
                FulcrumAssert.IsTrue(FulcrumApplication.IsInDevelopment, null,
                    "This property must only be used in unit tests.");
                return HasAliveBackgroundWorker;
            }
        }

        /// <summary>
        ///     This is a property specifically for unit testing.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private bool HasAliveBackgroundWorker => _backgroundWorkerThread != null && _backgroundWorkerThread.IsAlive;

        /// <inheritdoc />
        public string Name { get; }
    }

    public partial class MemoryQueue<T> : IWritableQueue<T>
    {
        /// <inheritdoc />
        public async Task ClearAsync()
        {
            if (_queue == null) return;
            while (_queue.TryDequeue(out _))
            {
            }

            await Task.Yield();
        }

        /// <inheritdoc />
        public Task AddMessageAsync(T message, TimeSpan? timeSpanToWait = null)
        {
            AddMessage(message, timeSpanToWait);
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="AddMessageAsync" />
        public void AddMessage(T message, TimeSpan? timeSpanToWait = null)
        {
            FulcrumAssert.IsNotNull(_queue, null, $"Expected the queue ({Name}) to exist.");
            var messageWithExpiration = new MessageEnvelope<T>(message, timeSpanToWait);
            _queue.Enqueue(messageWithExpiration);
            StartBackgroundWorkerIfNeeded();
        }

        /// <summary>
        ///     Starts a background worker if needed. The background worker will take log messages from the queue until it is
        ///     empty.
        /// </summary>
        private void StartBackgroundWorkerIfNeeded()
        {
            // It is optional to have a background worker
            if (_queueItemAction == null) return;
            lock (_queue)
            {
                if (_queue.IsEmpty || HasAliveBackgroundWorker) return;
                _backgroundWorkerThread = null;
                _backgroundWorkerThread =
                    ThreadHelper.FireAndForgetResetContext(async () =>
                        await BackgroundWorker().ConfigureAwait(false));
            }
        }

        private async Task BackgroundWorker()
        {
            while (!_queue.IsEmpty)
            {
                if (_actionsCanExecuteWithoutIndividualAwait)
                    await CallCallbackUntilQueueIsEmptyWithCollectionAwaitAsync();
                else
                    await CallCallbackWithIndividualAwaitUntilQueueIsEmptyAsync();

                // As it is somewhat expensive to start a new background worker, we will hang around 
                // for a short while to see if new items appear on the queue.
                await WaitForAdditionalItemsAsync(TimeSpan.FromMilliseconds(1000));
            }
            LatestItemFetchedAfterActiveTimeSpan = TimeSpan.Zero;
        }

        /// <summary>
        ///     Await every call to the callback method.
        /// </summary>
        private async Task CallCallbackWithIndividualAwaitUntilQueueIsEmptyAsync()
        {
            FulcrumAssert.IsTrue(!_actionsCanExecuteWithoutIndividualAwait);
            while (!_queue.IsEmpty)
            {
                var message = await GetOneMessageWithPossibleDelayAsync();
                if (Equals(message, default(T))) continue;
                try
                {
                    var messageIfException = $"QueueItemAction failed fore queue {Name} and item {message}.";
                    await ThreadHelper.ExecuteActionFailSafeAsync(async () => await _queueItemAction(message),
                        messageIfException);
                }
                catch (Exception e)
                {
                    LogHelper.FallbackSafeLog(
                        LogSeverityLevel.Error, 
                        $"This part of the code should be fail safe, but we still got an exception.",
                        e);
                }
            }
        }

        /// <summary>
        ///     Call all items in the queue asynchronously before awaiting them all.
        /// </summary>
        private async Task CallCallbackUntilQueueIsEmptyWithCollectionAwaitAsync()
        {
            FulcrumAssert.IsTrue(_actionsCanExecuteWithoutIndividualAwait);
            var taskList = new List<Task>();
            while (!_queue.IsEmpty)
            {
                var message = await GetOneMessageWithPossibleDelayAsync();
                if (Equals(message, default(T))) continue;
                var messageIfException = $"QueueItemAction failed fore queue {Name} and item {message}.";
                var task = ThreadHelper.ExecuteActionFailSafeAsync(async () => await _queueItemAction(message),
                    messageIfException);
                taskList.Add(task);
            }

            try
            {
                await Task.WhenAll(taskList);
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(
                    LogSeverityLevel.Error,
                    $"This part of the code should be fail safe, but we still got an exception.",
                    e);
            }
        }

        private async Task<T> GetOneMessageWithPossibleDelayAsync()
        {
            var message = await GetOneMessageNoBlockAsync();
            if (!_queue.IsEmpty && Equals(message, default(T))) await Task.Delay(TimeSpan.FromMilliseconds(1000));

            return message;
        }

        private async Task WaitForAdditionalItemsAsync(TimeSpan timeSpan)
        {
            var deadline = DateTimeOffset.Now.Add(timeSpan);
            while (DateTimeOffset.Now < deadline)
            {
                if (!_queue.IsEmpty) return;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }
    }

    public partial class MemoryQueue<T> : IQueueMetrics
    {
        /// <inheritdoc />
        public TimeSpan LatestItemFetchedAfterActiveTimeSpan { get; private set; }

        /// <inheritdoc />
        public DateTimeOffset? LatestItemFetchedAt { get; private set; }
    }

    public partial class MemoryQueue<T> : IReadableQueue<T>
    {
        /// <inheritdoc />
        public async Task<T> GetOneMessageNoBlockAsync()
        {
            var triedItems = new Dictionary<Guid, MessageEnvelope<T>>();
            while (true)
            {
                if (!_queue.TryDequeue(out var envelope))
                {
                    // Empty queue means no waiting time.
                    LatestItemFetchedAfterActiveTimeSpan = TimeSpan.Zero;
                    return default(T);
                }

                if (envelope.IsActivationTime)
                {
                    LatestItemFetchedAt = DateTimeOffset.Now;
                    LatestItemFetchedAfterActiveTimeSpan = envelope.ActiveTimeInQueue;
                    // We have found the first message in the queue that should be activated now
                    return await Task.FromResult(envelope.Message);
                }

                // Put the envelope back at the end of the queue
                _queue.Enqueue(envelope);
                if (triedItems.ContainsKey(envelope.Id))
                {
                    // We have looped through all waiting items in the queue
                    LatestItemFetchedAfterActiveTimeSpan = TimeSpan.Zero;
                    return default(T);
                }
                triedItems.Add(envelope.Id, envelope);
            }
        }
    }

    public partial class MemoryQueue<T> : IPeekableQueue<T>
    {
        /// <inheritdoc />
        public Task<T> PeekNoBlockAsync()
        {
            if (!_queue.TryPeek(out var item)) return Task.FromResult(default(T));
            return item.IsActivationTime ? Task.FromResult(item.Message) : Task.FromResult(default(T));
        }
    }


    public partial class MemoryQueue<T> : IResourceHealth
    {
        /// <inheritdoc />
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant)
        {
            return await Task.FromResult(new HealthResponse("MemoryQueue"));
        }
    }
}