using System;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CoolDownStrategy
    {
        public delegate TimeSpan CalculateCoolDownDelegate(int consecutiveFails);

        private readonly CalculateCoolDownDelegate _calculateCoolDown;

        private int _level;
        public DateTimeOffset LastFailAt { get; private set; }
        public DateTimeOffset NextTryAt { get; private set; }

        public bool HasCooledDown => DateTimeOffset.Now >= NextTryAt;

        public CoolDownStrategy(CalculateCoolDownDelegate calculateCoolDown)
        {
            _calculateCoolDown = calculateCoolDown;
        }

        public CoolDownStrategy(TimeSpan constant) : this(TimeSpan.MaxValue, constant, TimeSpan.Zero)
        {
        }

        public CoolDownStrategy(TimeSpan max, TimeSpan constant, TimeSpan coefficient) : this(max, constant, coefficient, 1.0)
        {
        }

        public CoolDownStrategy(TimeSpan max, TimeSpan constant, TimeSpan coefficient, double exponentiationBase)
        {
            if (coefficient == TimeSpan.Zero)
            {
                _calculateCoolDown = (consecutiveFails) => Constant(constant);
            }
            else if (exponentiationBase < 1.001 && exponentiationBase > 0.999)
            {
                _calculateCoolDown = (consecutiveFails) => Linear(consecutiveFails, max, constant, coefficient);
            }
            else
            {
                _calculateCoolDown = (consecutiveFails) => Exponential(consecutiveFails, max, constant, coefficient, exponentiationBase);
            }
            Reset();
        }

        public void Reset()
        {
            _level = 0;
            NextTryAt = DateTimeOffset.MaxValue;
        }

        public void Next()
        {
            LastFailAt = DateTimeOffset.Now;
            _level++;
            NextTryAt = LastFailAt + _calculateCoolDown(_level);
        }

        public void Start()
        {
            Reset();
            Next();
        }

        public static TimeSpan Constant(TimeSpan constant)
        {
            return constant;
        }

        /// <summary>
        /// <paramref name="constant"/> + <paramref name="coefficient"/> * (<paramref name="consecutiveFails"/> - 1)
        /// If the calculated value is greater than <paramref name="max"/>, then max is returned.
        /// </summary>
        public static TimeSpan Linear(int consecutiveFails, TimeSpan max, TimeSpan constant, TimeSpan coefficient)
        {
            InternalContract.RequireGreaterThan(0, consecutiveFails, nameof(consecutiveFails));
            var calculatedTimeSpan = TimeSpan.FromSeconds(constant.TotalSeconds + (consecutiveFails - 1) * coefficient.TotalSeconds);
            return calculatedTimeSpan < max ? calculatedTimeSpan : max;
        }

        /// <summary>
        /// <paramref name="constant"/> + <paramref name="coefficient"/> * Math.Pow(<paramref name="exponentiationBase"/>, <paramref name="consecutiveFails"/> - 1)
        /// If the calculated value is greater than <paramref name="max"/>, then max is returned.
        /// </summary>
        public static TimeSpan Exponential(int consecutiveFails, TimeSpan max, TimeSpan constant, TimeSpan coefficient, double exponentiationBase = 2.0)
        {
            InternalContract.RequireGreaterThan(0, consecutiveFails, nameof(consecutiveFails));
            var calculatedTimeSpan = TimeSpan.FromSeconds(constant.TotalSeconds + Math.Pow(exponentiationBase, consecutiveFails - 1) * coefficient.TotalSeconds);
            return calculatedTimeSpan < max ? calculatedTimeSpan : max;
        }
    }
}
