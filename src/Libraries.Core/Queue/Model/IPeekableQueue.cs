using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    /// <summary>
    /// A generic interface for reading items to a queue.
    /// </summary>
    public interface IPeekableQueue<T> : IBaseQueue
    {
        /// <summary>
        /// Returns the front item from the queue without removing it from the queue, or null if no items are on the queue.
        /// </summary>
        Task<T> PeekNoBlockAsync(CancellationToken cancellationToken = default);
    }
}