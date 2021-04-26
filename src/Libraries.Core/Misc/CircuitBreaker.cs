using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    /// <inheritdoc />
    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly CircuitBreakerOptions _options;
        private StateEnum _state = StateEnum.Ok;

        protected readonly object Lock = new object();
        protected int ConcurrencyCount;
        private bool _concurrentRequestHadSuccess;
        private bool _latestRequestHadSuccess;
        private CancellationTokenSource _commonCancellationTokenSource;

        protected Exception LatestException { get; private set; }

        public enum StateEnum
        {
            Ok,
            ContenderIsTrying,
            Failed,
        }

        /// <inheritdoc />
        public DateTimeOffset LastFailAt => _options.CoolDownStrategy.CurrentCoolDownStartedAt;

        /// <inheritdoc />
        public virtual bool IsActive => _state != StateEnum.Ok || ConcurrencyCount == 0;

        /// <inheritdoc />
        public DateTimeOffset? FirstFailureAt { get; private set; }

        /// <inheritdoc />
        public int ConsecutiveContenderErrors { get; private set; }

        /// <inheritdoc />
        public bool IsRefusing => _state != StateEnum.Ok;

        /// <inheritdoc />
        public void ForceEndOfCoolDown() => _options.CoolDownStrategy?.ForceEndOfCoolDown();

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitBreaker(CircuitBreakerOptions options)
        {
            InternalContract.RequireNotNull(options, nameof(options));
            InternalContract.RequireValidated(options, nameof(options));
            _options = options;
            _options.CoolDownStrategy.Reset();
            _commonCancellationTokenSource = new CancellationTokenSource();
        }

        protected virtual bool IsQuickFailRecommended()
        {
            lock (Lock)
            {
                switch (_state)
                {
                    case StateEnum.Ok:
                        return false;
                    case StateEnum.Failed:
                        // Wait for all other requests to be done.
                        var othersAreRunning = ConcurrencyCount > 1;
                        if (othersAreRunning) return true;
                        // Now all the concurrent requests are done. Create a new common token for concurrent invocations.
                        _commonCancellationTokenSource = new CancellationTokenSource();
                        // If the last request was successful, we can go to state OK without
                        // trying with a contender
                        if (_latestRequestHadSuccess)
                        {
                            _concurrentRequestHadSuccess = false;
                            _state = StateEnum.Ok;
                            return false;
                        }
                        // Cool down until we try again. Note! If one of the concurrent requests after the fail was detected
                        // had a success, we don't wait for cool down.
                        if (!_concurrentRequestHadSuccess && !_options.CoolDownStrategy.HasCooledDown) return true;
                        // When the time has come to do another try, we will let the first contender through.
                        _concurrentRequestHadSuccess = false;
                        _state = StateEnum.ContenderIsTrying;
                        return false;
                    case StateEnum.ContenderIsTrying:
                        // We have a contender. Deny everyone else.
                        return true;
                    default:
                        FulcrumAssert.Fail($"Unknown {typeof(StateEnum).FullName}: {_state}.");
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected virtual void ReportFailure(Exception exception)
        {
            lock (Lock)
            {
                _latestRequestHadSuccess = false;
                FirstFailureAt = FirstFailureAt ?? DateTimeOffset.UtcNow;
                if (_state == StateEnum.ContenderIsTrying) ConsecutiveContenderErrors++;
                _state = StateEnum.Failed;
                LatestException = exception;
                if (_options.CancelConcurrentRequestsWhenOneFails)
                {
                    // Cancel all concurrent calls
                    _commonCancellationTokenSource.Cancel();
                }
                _options.CoolDownStrategy.StartNextCoolDownPeriod();
            }
        }

        protected virtual void ReportSuccess()
        {
            if (_state == StateEnum.Ok) return;

            lock (Lock)
            {
                if (_state == StateEnum.Failed)
                {
                    _concurrentRequestHadSuccess = true;
                    _latestRequestHadSuccess = true;
                    return;
                }
                if (_state == StateEnum.ContenderIsTrying) ConsecutiveContenderErrors = 0;
                _state = StateEnum.Ok;
                FirstFailureAt = null;
                LatestException = null;
                _options.CoolDownStrategy.Reset();
            }
        }

        /// <inheritdoc />
        public virtual async Task ExecuteOrThrowAsync(Func<CancellationToken, Task> requestAsync, CancellationToken cancellationToken = default)
        {
            await ExecuteOrThrowAsync(async (t) =>
            {
                await requestAsync(cancellationToken);
                return true;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<T> ExecuteOrThrowAsync<T>(Func<CancellationToken, Task<T>> requestAsync, CancellationToken cancellationToken = default)
        {
            try
            {
                Interlocked.Increment(ref ConcurrencyCount);

                if (IsQuickFailRecommended())
                {
                    FulcrumAssert.IsNotNull(LatestException, CodeLocation.AsString());
                    throw LatestException;
                }


                CancellationToken token;

                token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                    _commonCancellationTokenSource.Token).Token;
                token.ThrowIfCancellationRequested();

                try
                {
                    var result = await requestAsync(token);
                    ReportSuccess();
                    return result;
                }
                catch (CircuitBreakerException e)
                {
                    FulcrumAssert.IsNotNull(e.InnerException, CodeLocation.AsString());
                    ReportFailure(e.InnerException);
                    // ReSharper disable once PossibleNullReferenceException
                    throw e.InnerException;
                }
            }
            finally
            {
                Interlocked.Decrement(ref ConcurrencyCount);
            }
        }
    }
}
