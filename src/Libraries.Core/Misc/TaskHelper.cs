using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class TaskHelper
    {
        private static readonly Random Random = new Random();

        public static Task RandomDelayAsync(TimeSpan minTime, TimeSpan maxTime, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(TimeSpan.Zero, minTime, nameof(minTime));
            InternalContract.RequireGreaterThanOrEqualTo(minTime, maxTime, nameof(maxTime));

            var minTimeInSeconds = minTime.TotalSeconds;
            var maxTimeInSeconds = maxTime.TotalSeconds;
            var interval = maxTimeInSeconds - minTimeInSeconds;

            var waitTimeInSeconds = minTimeInSeconds + Random.NextDouble() * interval;
            var waitTime = TimeSpan.FromSeconds(waitTimeInSeconds);

            return Task.Delay(waitTime, cancellationToken);
        }
    }
}
