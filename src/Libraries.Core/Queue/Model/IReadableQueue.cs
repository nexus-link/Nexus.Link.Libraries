using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    /// <summary>
    /// A generic interface for reading items to a queue.
    /// </summary>
    public interface IReadableQueue<T> : IBaseQueue
    {
        /// <summary>
        /// Remove all items from the queue
        /// </summary>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns one item from the queue, or null if no items are on the queue.
        /// </summary>
        Task<T> GetOneMessageNoBlockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// If no items are on the queue return null, otherwise pass the message to <paramref name="action"/>
        /// and return the result.
        /// If <paramref name="action"/> throws an exception, the message will be left on the queue
        /// and the exception will be trown upwards.
        /// When a message is left on the queue, it is preferrably left at the top of the queue, but if that
        /// can't be accomplished it is OK to be left anywhere in the queue.
        /// </summary>
        /// <remarks>
        /// <paramref name="action"/> may (in rare conditions) be called more than once for the same queue item.
        /// </remarks>
        Task<T> GetOneMessageNoBlockAsync(Func<T, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
    }
}