using System;
using System.Runtime.InteropServices;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreakerWithThrottling : CircuitBreaker
    {
        private readonly CoolDown _chokingCoolDown;
        private readonly int _thresholdConcurrency;
        private int _maxConcurrency;
        private int _concurrencyCount;
        private bool _choked;

        public CircuitBreakerWithThrottling(CoolDown errorCoolDown, CoolDown chokingCoolDown = null, int thresholdConcurrency = 0)
        : base(errorCoolDown)
        {
            _chokingCoolDown = chokingCoolDown;
            _thresholdConcurrency = thresholdConcurrency;
        }

        protected override bool IsQuickFailRecommended()
        {
            lock (Lock)
            {
                if (base.IsQuickFailRecommended()) return true;
                if (!_choked) return false;
                if (_chokingCoolDown.HasCooledDown)
                {
                    _maxConcurrency++;
                    _chokingCoolDown.Increase();
                }

                if (_maxConcurrency > _thresholdConcurrency)
                {
                    _choked = false;
                    _maxConcurrency = 0;
                    _chokingCoolDown.Reset();
                    return false;
                }

                if (_concurrencyCount >= _maxConcurrency) return true;
                _concurrencyCount++;
                return false;
            }
        }

        public virtual void ReportChoking()
        {
            lock (Lock)
            {
                _choked = true;
                _maxConcurrency = 1;
                _chokingCoolDown.Reset();
                _chokingCoolDown.Increase();
            }
        }
    }
}
