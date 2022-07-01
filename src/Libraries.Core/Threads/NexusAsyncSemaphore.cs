using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Threads
{
    /// <summary>
    /// Support for running a maximum number of things in parallel.
    /// </summary>
    public class NexusAsyncSemaphore
    {
        private readonly int _maximumCount;
        private readonly TimeSpan _timeSpanBetweenRetries;
        private readonly object _lock = new object();
        private int _count;

        private static AsyncLocal<int> _insideLockLevel = new AsyncLocal<int> { Value = 0 };
        /// <summary>
        /// A semaphore with maximumCount of 1.
        /// </summary>
        /// <param name="timeSpanBetweenRetries">The time to wait between new tries to raise the semaphore.
        /// The minimum wait time is (1 ms).
        /// Null means that we will use the minimum wait time.</param>
        public NexusAsyncSemaphore(TimeSpan? timeSpanBetweenRetries = null) : this(1, timeSpanBetweenRetries)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maximumCount">The maximum number of parallel executions for this semaphore.</param>
        /// <param name="timeSpanBetweenRetries">The time to wait between new tries to raise the semaphore.
        /// The minimum wait time is (1 ms).
        /// Null means that we will use the minimum wait time.</param>
        public NexusAsyncSemaphore(int maximumCount, TimeSpan? timeSpanBetweenRetries = null)
        {
            _maximumCount = maximumCount;
            InternalContract.RequireGreaterThanOrEqualTo(1, maximumCount, nameof(maximumCount));
            if (timeSpanBetweenRetries == null) timeSpanBetweenRetries = TimeSpan.FromMilliseconds(1);
            InternalContract.RequireGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(1), timeSpanBetweenRetries.Value, nameof(timeSpanBetweenRetries));
            _timeSpanBetweenRetries = timeSpanBetweenRetries.Value;
            _count = 0;
            _insideLockLevel.Value = 0;
        }

        /// <summary>
        /// Raise the semaphore. If it was already raised, we will wait until it is lowered.
        /// </summary>
        /// <remarks>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// Note! You must use 'try {await semaphore.Raise(); ... your code ...} finally {await semaphore.Lower()}'
        /// to be certain that the semaphore is lowered, or you risk to end up in an endless loop.</remarks>
        public async Task RaiseAsync(CancellationToken token = default)
        {
            lock (_lock)
            {
                if (_insideLockLevel.Value > 0) return;
            }

            while (true)
            {
                lock (_lock)
                {
                    if (_count < _maximumCount)
                    {
                        _count++;
                        break;
                    }
                }
                await Task.Delay(_timeSpanBetweenRetries, token);
            }
        }

        /// <summary>
        /// Lower the semaphore. NOTE! Use this in a 'try-finally' statement to be absolutely
        /// sure that we eventually lower the semaphore. See <see cref="RaiseAsync"/>.
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// </summary>
        [Obsolete("Use Lower(). Obsolete since 2021-10-07.")]
        public Task LowerAsync(CancellationToken token = default)
        {
            Lower();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Lower the semaphore. NOTE! Use this in a 'try-finally' statement to be absolutely
        /// sure that we eventually lower the semaphore. See <see cref="RaiseAsync"/>.
        /// </summary>
        public void Lower()
        {
            lock (_lock)
            {
                if (_insideLockLevel.Value > 0) return;
                InternalContract.Require(_count > 0, $"The semaphore was already at count {_count}, so you can't lower it again.");
                _count--;
            }
        }

        /// <summary>
        /// Execute <paramref name="asyncMethod"/> with the semaphore raised,
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// i.e. parallel calls to this method will execute one at a time.
        /// </summary>
        /// <param name="asyncMethod">The method to execute with restricted parallelism.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default)
        {
            try
            {
                await RaiseAsync(token);
                await NestedAsync(asyncMethod, token);
            }
            finally
            {
                Lower();
            }
        }

        private async Task NestedAsync(Func<CancellationToken, Task> asyncMethod, CancellationToken token)
        {
            lock (_lock)
            {
                _insideLockLevel.Value++;
            }
            await asyncMethod(token);
            lock (_lock)
            {
                _insideLockLevel.Value--;
            }
        }

        /// <summary>
        /// Execute <paramref name="asyncMethod"/> with the semaphore raised,
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// i.e. parallel calls to this method will execute one at a time.
        /// </summary>
        /// <param name="asyncMethod">The method to execute with restricted parallelism.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> asyncMethod, CancellationToken token = default)
        {
            try
            {
                await RaiseAsync(token);
                return await asyncMethod(token);
            }
            finally
            {
                Lower();
            }
        }

        /// <summary>
        /// Execute <paramref name="asyncMethod"/> with the semaphore raised,
        /// i.e. parallel calls to this method will execute one at a time.
        /// </summary>
        /// <param name="asyncMethod">The method to execute with restricted parallelism.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task ExecuteAsync(Func<Task> asyncMethod, CancellationToken token = default)
        {
            return ExecuteAsync((t) => asyncMethod(), token);
        }

        /// <summary>
        /// Execute <paramref name="asyncMethod"/> with the semaphore raised,
        /// i.e. parallel calls to this method will execute one at a time.
        /// </summary>
        /// <param name="asyncMethod">The method to execute with restricted parallelism.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task<T> ExecuteAsync<T>(Func<Task<T>> asyncMethod, CancellationToken token = default)
        {
            return ExecuteAsync((t) => asyncMethod(), token);
        }
    }
}
