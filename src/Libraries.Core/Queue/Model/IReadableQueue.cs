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
        Task ClearAsync();

        /// <summary>
        /// Returns one item from the queue, or null if no items are on the queue.
        /// </summary>
        Task<T> GetOneMessageNoBlockAsync();
    }
}