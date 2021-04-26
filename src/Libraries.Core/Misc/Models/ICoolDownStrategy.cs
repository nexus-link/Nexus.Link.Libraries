using System;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public interface ICoolDownStrategy
    {
        /// <summary>
        /// The time when the current cool down period started.
        /// </summary>
        DateTimeOffset CurrentCoolDownStartedAt { get; }

        /// <summary>
        /// The time when the current cool down period will pass.
        /// </summary>
        DateTimeOffset NextTryAt { get; }

        /// <summary>
        /// True if the cool down period has passed.
        /// </summary>
        bool HasCooledDown { get; }

        /// <summary>
        /// Reset the cooldown. <see cref="HasCooledDown"/>will be false.
        /// Use <see cref="StartNextCoolDownPeriod"/> to start all over.
        /// </summary>
        void Reset();

        /// <summary>
        /// Override the cool down period, meaning HasCooledDown == true immediately
        /// </summary>
        void ForceEndOfCoolDown();

        /// <summary>
        /// Calculate and start the next cool down period.
        /// </summary>
        void StartNextCoolDownPeriod();
    }
}