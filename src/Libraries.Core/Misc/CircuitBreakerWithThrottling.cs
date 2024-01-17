using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreakerWithThrottling : CircuitBreaker
    {
        private readonly CircuitBreakerWithThrottlingOptions _options;
        private Exception _latestChokeException;
        private int _maxConcurrency;
        private bool _choked;

        /// <inheritdoc />
        public override bool IsActive => _choked || base.IsActive;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitBreakerWithThrottling(CircuitBreakerWithThrottlingOptions options)
        : base(options)
        {
            InternalContract.RequireNotNull(options, nameof(options));
            InternalContract.RequireValidated(options, nameof(options));
            _options = options;
            _options.ThrottlingCoolDownStrategy.Reset();
        }

        /// <summary>
        /// Returns true if we are in a choking situation and there are too many concurrent requests.
        /// </summary>
        protected bool IsThrottlingRecommended()
        {
            lock (Lock)
            {
                if (!_choked) return false;

                // Is it time to increase the allowed number of concurrent requests?
                if (_options.ThrottlingCoolDownStrategy.HasCooledDown)
                {
                    _maxConcurrency++;
                    _options.ThrottlingCoolDownStrategy.StartNextCoolDownPeriod();

                    // If we reached the threshold, then we consider this as not a choke situation anymore
                    if (_maxConcurrency >= _options.ConcurrencyThresholdForChokingResolved)
                    {
                        _choked = false;
                        _maxConcurrency = 0;
                        _latestChokeException = null;
                        _options.ThrottlingCoolDownStrategy.Reset();
                        return false;
                    }
                }

                return ConcurrencyCount > _maxConcurrency;
            }
        }

        /// <summary>
        /// Call this when we have a choking situation.
        /// As a result, the circuit breaker will go down to only allowing one request at a time, and then gradually
        /// increase the number of concurrent requests.
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void ReportChoked(Exception exception)
        {
            lock (Lock)
            {
                _choked = true;
                _maxConcurrency = 1;
                _latestChokeException = exception;
                _options.ThrottlingCoolDownStrategy.Reset();
                _options.ThrottlingCoolDownStrategy.StartNextCoolDownPeriod();
            }
        }

        /// <inheritdoc />
        public override async Task<T> ExecuteOrThrowAsync<T>(Func<CancellationToken, Task<T>> requestAsync, CancellationToken cancellationToken = default)
        {
            try
            {
                Interlocked.Increment(ref ConcurrencyCount);
                if (IsQuickFailRecommended())
                {
                    FulcrumAssert.IsNotNull(LatestException, CodeLocation.AsString());
                    throw LatestException;
                }

                if (IsThrottlingRecommended())
                {
                    FulcrumAssert.IsNotNull(_latestChokeException, CodeLocation.AsString());
                    throw _latestChokeException;
                }

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var result = await requestAsync(cancellationToken);
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
                catch (ChokeException e)
                {
                    FulcrumAssert.IsNotNull(e.InnerException, CodeLocation.AsString());
                    ReportChoked(e.InnerException);
                    // ReSharper disable once PossibleNullReferenceException
                    throw e.InnerException;
                }
            }
            finally
            {
                Interlocked.Decrement(ref ConcurrencyCount);
            }
        }

        /// <inheritdoc />
        public override T ExecuteOrThrow<T>(Func<T> function)
        {
            try
            {
                Interlocked.Increment(ref ConcurrencyCount);
                if (IsQuickFailRecommended())
                {
                    FulcrumAssert.IsNotNull(LatestException, CodeLocation.AsString());
                    throw LatestException;
                }

                if (IsThrottlingRecommended())
                {
                    FulcrumAssert.IsNotNull(_latestChokeException, CodeLocation.AsString());
                    throw _latestChokeException;
                }

                try
                {
                    var result = function();
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
                catch (ChokeException e)
                {
                    FulcrumAssert.IsNotNull(e.InnerException, CodeLocation.AsString());
                    ReportChoked(e.InnerException);
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
