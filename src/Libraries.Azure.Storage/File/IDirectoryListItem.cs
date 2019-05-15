using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Azure.Storage.File
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IDirectoryListItem : ILoggable
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The Uri for the object.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Delete the object physically from the file system.
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();
    }
}
