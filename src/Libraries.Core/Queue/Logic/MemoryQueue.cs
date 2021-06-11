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
                ? (DateTimeOffset?)null
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
        public delegate Task QueueItemActionDelegate(T item, CancellationToken cancellationToken = default);

        private bool _actionsCanExecuteWithoutIndividualAwait;
        private readonly ConcurrentQueue<MessageEnvelope<T>> _queue;
        private QueueItemActionDelegate _queueItemAction;
        private Thread _backgroundWorkerThread;

        public TimeSpan KeepQueueAliveTimeSpan { get; set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MemoryQueue(string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            Name = name;
            _queue = new ConcurrentQueue<MessageEnvelope<T>>();
            KeepQueueAliveTimeSpan = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        [Obsolete("Use the MemoryQueue(string) constructor and then call SetQueueItemAction(). Obsolete since 2021-01-20", false)]
        public MemoryQueue(string name, QueueItemActionDelegate queueItemAction) : this(name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            SetQueueItemAction(queueItemAction);
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
        [Obsolete("Use the MemoryQueue(string) constructor and then call SetQueueItemAction(). Obsolete since 2021-01-20", false)]
        public MemoryQueue(string name, QueueItemActionDelegate queueItemAction = null,
            bool actionsCanExecuteWithoutIndividualAwait = false) : this(name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            SetQueueItemAction(queueItemAction, actionsCanExecuteWithoutIndividualAwait);
        }

        /// <summary>
        /// Set a method that should be executed for every item in the queue.
        /// </summary>
        /// <param name="queueItemAction">Optional action that will be called for every item taken from the queue.</param>
        /// <param name="actionsCanExecuteWithoutIndividualAwait">
        ///     True means that the many calls can be made to the
        ///     <paramref name="queueItemAction" /> action without awaiting every single one.
        /// </param>
        /// <remarks>
        ///     If <paramref name="actionsCanExecuteWithoutIndividualAwait" /> is true, then you guarantee that it is possible to
        ///     run many <paramref name="queueItemAction" /> "in parallel" without interference.
        /// </remarks>
        public void SetQueueItemAction(QueueItemActionDelegate queueItemAction,
            bool actionsCanExecuteWithoutIndividualAwait = false)
        {
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
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            if (_queue == null) return;
            while (_queue.TryDequeue(out _))
            {
            }

            await Task.Yield();
        }

        /// <inheritdoc />
        public Task AddMessageAsync(T message, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default)
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
                        await BackgroundWorkerAsync(CancellationToken.None).ConfigureAwait(false));
            }
        }

        private async Task BackgroundWorkerAsync(CancellationToken cancellationToken)
        {
            while (!_queue.IsEmpty)
            {
                if (_actionsCanExecuteWithoutIndividualAwait)
                    await CallCallbackUntilQueueIsEmptyWithCollectionAwaitAsync(cancellationToken);
                else
                    await CallCallbackWithIndividualAwaitUntilQueueIsEmptyAsync(cancellationToken);

                // As it is somewhat expensive to start a new background worker, we will hang around 
                // for a short while to see if new items appear on the queue.
                await WaitForAdditionalItemsAsync(KeepQueueAliveTimeSpan, cancellationToken);
            }
            LatestItemFetchedAfterActiveTimeSpan = TimeSpan.Zero;
        }

        /// <summary>
        ///     Await every call to the callback method.
        /// </summary>
        private async Task CallCallbackWithIndividualAwaitUntilQueueIsEmptyAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsTrue(!_actionsCanExecuteWithoutIndividualAwait);
            while (!_queue.IsEmpty)
            {
                var message = await GetOneMessageWithPossibleDelayAsync(cancellationToken);
                if (Equals(message, default(T))) continue;
                try
                {
                    var messageIfException = $"QueueItemAction failed fore queue {Name} and item {message}.";
                    await ThreadHelper.ExecuteActionFailSafeAsync(async () => await _queueItemAction(message, cancellationToken),
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
        private async Task CallCallbackUntilQueueIsEmptyWithCollectionAwaitAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsTrue(_actionsCanExecuteWithoutIndividualAwait);
            var previousTaskList = new List<Task>();
            var currentTaskList = new List<Task>();
            var currentTasksCount = 0;
            while (!_queue.IsEmpty)
            {
                var message = await GetOneMessageNoBlockAsync(cancellationToken);
                if (Equals(message, default(T)))
                {
                    if (_queue.IsEmpty) break;
                    // We have items in the queue that are waiting for the correct time, so wait a second
                    currentTaskList.Add(Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken));
                    await AwaitAllFailSafeAsync(previousTaskList, cancellationToken);
                    await AwaitAllFailSafeAsync(currentTaskList, cancellationToken);
                    continue;
                }
                var messageIfException = $"QueueItemAction failed fore queue {Name} and item {message}.";
                var task = ThreadHelper.ExecuteActionFailSafeAsync(async () => await _queueItemAction(message, cancellationToken),
                    messageIfException);
                currentTaskList.Add(task);
                currentTasksCount++;
                if (currentTasksCount < 10000) continue;
                // If we have a VERY long queue, there is a risk that the task list will grow for ever.
                // The strategy to solve this is to have two task lists. When the task list has reached its max, we will archive it.
                // The next time that the task list has reached its max, we will await the archived tasks. The plan
                // here is that the archived tasks should now have completed (when we have been filling the current task list),
                // so awaiting them will not make the execution pause.
                await AwaitAllFailSafeAsync(previousTaskList, cancellationToken);
                previousTaskList = currentTaskList;
                currentTaskList = new List<Task>();
                currentTasksCount = 0;
            }

            await AwaitAllFailSafeAsync(previousTaskList, cancellationToken);
            await AwaitAllFailSafeAsync(currentTaskList, cancellationToken);
        }

        private static async Task AwaitAllFailSafeAsync(ICollection<Task> taskList, CancellationToken cancellationToken)
        {
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
            finally
            {
                taskList.Clear();
            }
        }

        private async Task<T> GetOneMessageWithPossibleDelayAsync(CancellationToken cancellationToken)
        {
            var message = await GetOneMessageNoBlockAsync(cancellationToken);
            if (!_queue.IsEmpty && Equals(message, default(T))) await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);

            return message;
        }

        private async Task WaitForAdditionalItemsAsync(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            var deadline = DateTimeOffset.Now.Add(timeSpan);
            while (DateTimeOffset.Now < deadline)
            {
                if (!_queue.IsEmpty) return;
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
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

    public partial class MemoryQueue<T> : ICountableQueue
    {
        public async Task<int?> GetApproximateMessageCountAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_queue.Count);
        }
    }

    public partial class MemoryQueue<T> : IReadableQueue<T>
    {
        /// <inheritdoc />
        public async Task<T> GetOneMessageNoBlockAsync(CancellationToken cancellationToken = default)
        {
            var triedItems = new Dictionary<Guid, MessageEnvelope<T>>();
            while (true)
            {
                if (!_queue.TryDequeue(out var envelope))
                {
                    // Empty queue means no waiting time.
                    LatestItemFetchedAfterActiveTimeSpan = TimeSpan.Zero;
                    return default;
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
                    return default;
                }
                triedItems.Add(envelope.Id, envelope);
            }
        }
    }

    public partial class MemoryQueue<T> : IPeekableQueue<T>
    {
        /// <inheritdoc />
        public Task<T> PeekNoBlockAsync(CancellationToken cancellationToken = default)
        {
            if (!_queue.TryPeek(out var item)) return Task.FromResult(default(T));
            return item.IsActivationTime ? Task.FromResult(item.Message) : Task.FromResult(default(T));
        }
    }


    public partial class MemoryQueue<T> : IResourceHealth
    {
        /// <inheritdoc />
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new HealthResponse("MemoryQueue"));
        }
    }
}