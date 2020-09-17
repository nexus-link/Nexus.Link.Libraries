using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Azure.Storage.Queue;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    /// <summary>
    /// A generic interface for reading items to a queue.
    /// </summary>
    [Obsolete("This was a dead end. Obsolete since 2019-12-03.", true)]
    public interface IAzureStorageQueue<T> : ICompleteQueue<T> where T : IAzureStorageQueueMessage
    {
        /// <summary>
        /// Removes the specified message from the queue
        /// </summary>
        Task DeleteMessageAsync(T message);
    }
}