using System.Threading.Tasks;
using Nexus.Link.Libraries.Azure.Storage.Queue;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    /// <summary>
    /// A generic interface for reading items to a queue.
    /// </summary>
    public interface IAzureStorageQueue<in T> : IBaseQueue where T : IAzureStorageQueueMessage
    {
        /// <summary>
        /// Removes the specified message from the queue
        /// </summary>
        Task DeleteMessageAsync(T message);
    }
}