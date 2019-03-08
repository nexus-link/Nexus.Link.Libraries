using System;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    public interface IQueueMetrics
    {
        /// <summary>
        /// Updated every time an item has been fetched from the queue.
        /// The value is the time span that the item has been actively waiting to be fetched on the queue.
        /// As soon as the queue is empty, this is set to 0.
        /// </summary>
        TimeSpan LatestItemFetchedAfterActiveTimeSpan { get; }

        /// <summary>
        /// Updated every time an item has been fetched from the queue.
        /// The value is the time when the item was fetched from the queue.
        /// The value is null if no items has yet been fetched.
        /// </summary>
        DateTimeOffset? LatestItemFetchedAt { get; }
    }
}
