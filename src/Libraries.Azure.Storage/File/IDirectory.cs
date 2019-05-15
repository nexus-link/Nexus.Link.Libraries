using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Storage.File
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IDirectory : IDirectoryListItem
    {
        /// <summary>
        /// Create a file for this directory. 
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <returns>A <see cref="IFile"/> object.</returns>
        /// <remarks>Please note. The file will not be created physically, that will have to be done explicitly with methods for the returned object.</remarks>
        IFile CreateFile(string name);

        /// <summary>
        /// Return true if the directory exists already.
        /// </summary>
        /// <returns></returns>
        Task<bool> ExistsAsync();

        /// <summary>
        /// If this directory does not exist, create it.
        /// </summary>
        Task CreateIfNotExistsAsync();

        /// <summary>
        /// List the files of a directory
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<IDirectoryListItem>> ListContentAsync(CancellationToken ct = default(CancellationToken));
    }
}
