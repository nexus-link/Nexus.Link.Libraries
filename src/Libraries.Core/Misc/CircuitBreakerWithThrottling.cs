using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreakerWithThrottling : CircuitBreaker
    {
        private Exception _latestChokeException;
        private readonly ICoolDownStrategy _chokingCoolDownStrategy;
        private readonly int _thresholdConcurrency;
        private int _maxConcurrency;
        private bool _choked;

        /// <inheritdoc />
        public override bool IsActive => _choked || base.IsActive;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorCoolDownStrategy">Cool down strategy for non-choking errors.</param>
        /// <param name="chokingCoolDownStrategy">Cool down strategy for choking situations.</param>
        /// <param name="thresholdConcurrency">When we reach this level of concurrency, the choking situation is considered over.</param>
        public CircuitBreakerWithThrottling(ICoolDownStrategy errorCoolDownStrategy, ICoolDownStrategy chokingCoolDownStrategy, int thresholdConcurrency)
        : base(errorCoolDownStrategy)
        {
            InternalContract.RequireNotNull(chokingCoolDownStrategy, nameof(chokingCoolDownStrategy));
            InternalContract.RequireGreaterThan(1, thresholdConcurrency, nameof(thresholdConcurrency));
            _chokingCoolDownStrategy = chokingCoolDownStrategy;
            _thresholdConcurrency = thresholdConcurrency;
            _chokingCoolDownStrategy.Reset();
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
                if (_chokingCoolDownStrategy.HasCooledDown)
                {
                    _maxConcurrency++;
                    _chokingCoolDownStrategy.StartNextCoolDownPeriod();

                    // If we reached the threshold, then we consider this as not a choke situation anymore
                    if (_maxConcurrency > _thresholdConcurrency)
                    {
                        _choked = false;
                        _maxConcurrency = 0;
                        _latestChokeException = null;
                        _chokingCoolDownStrategy.Reset();
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
                _chokingCoolDownStrategy.Reset();
                _chokingCoolDownStrategy.StartNextCoolDownPeriod();
            }
        }

        /// <inheritdoc />
        public override async Task<T> ExecuteOrThrowAsync<T>(Func<Task<T>> requestAsync)
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
                    var result = await requestAsync();
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
