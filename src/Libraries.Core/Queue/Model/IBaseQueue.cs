using Nexus.Link.Libraries.Core.Health.Model;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IBaseQueue : IResourceHealth, IResourceHealth2
    {
        /// <summary>
        /// The name of the queue.
        /// </summary>
        string Name { get; }
    }
}
