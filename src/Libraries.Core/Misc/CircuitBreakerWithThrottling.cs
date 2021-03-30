using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreakerWithThrottling : CircuitBreaker
    {
        private Exception _latestChokeException;
        private readonly CoolDownStrategy _chokingCoolDownStrategy;
        private readonly int _thresholdConcurrency;
        private int _maxConcurrency;
        private int _concurrencyCount;
        private bool _choked;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorCoolDownStrategy">Cool down strategy for non-choking errors.</param>
        /// <param name="chokingCoolDownStrategy">Cool down strategy for choking situations.</param>
        /// <param name="thresholdConcurrency">When we reach this level of concurrency, the choking situation is considered over.</param>
        public CircuitBreakerWithThrottling(CoolDownStrategy errorCoolDownStrategy, CoolDownStrategy chokingCoolDownStrategy, int thresholdConcurrency)
        : base(errorCoolDownStrategy)
        {
            InternalContract.RequireNotNull(chokingCoolDownStrategy, nameof(chokingCoolDownStrategy));
            InternalContract.RequireGreaterThan(1, thresholdConcurrency, nameof(thresholdConcurrency));
            _chokingCoolDownStrategy = chokingCoolDownStrategy;
            _thresholdConcurrency = thresholdConcurrency;
            _chokingCoolDownStrategy.Reset();
        }

        /// <inheritdoc />
        public override async Task ExecuteOrThrowAsync(Func<Task> action)
        {
            try
            {
                _concurrencyCount++;
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
                    await action();
                    ReportSuccess();
                }
                catch (CircuitBreakerException e)
                {
                    ReportFailure(e);
                    throw;
                }
                catch (ChokeException e)
                {
                    ReportChoked(e);
                    throw;
                }
            }
            finally
            {
                _concurrencyCount--;
            }
        }

        protected bool IsThrottlingRecommended()
        {
            lock (Lock)
            {
                if (_choked && _chokingCoolDownStrategy.HasCooledDown)
                {
                    _maxConcurrency++;
                    _chokingCoolDownStrategy.Next();

                    if (_maxConcurrency > _thresholdConcurrency)
                    {
                        _choked = false;
                        _maxConcurrency = 0;
                        _latestChokeException = null;
                        _chokingCoolDownStrategy.Reset();
                    }
                }

                return _choked && _concurrencyCount > _maxConcurrency;
            }
        }

        protected virtual void ReportChoked(ChokeException exception)
        {
            lock (Lock)
            {
                _choked = true;
                _maxConcurrency = 1;
                _latestChokeException = exception;
                _chokingCoolDownStrategy.Reset();
                _chokingCoolDownStrategy.Next();
            }
        }
    }
}
