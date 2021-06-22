#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Web.AspNet.Queue
{
    public interface IStoppableQueue<T>
    {
        Task EnqueueAsync(T item, CancellationToken cancellationToken);

        Task<T> DequeueAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop the queue, i.e. don't deliver any more items in <see cref="DequeueAsync"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
#endif