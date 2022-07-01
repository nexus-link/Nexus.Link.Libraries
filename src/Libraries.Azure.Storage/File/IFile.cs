using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Storage.File
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IFile : IDirectoryListItem
    {
        /// <summary>
        /// Upload <paramref name="content"/> to the file.
        /// </summary>
        /// <param name="content">The content for the file.</param>
        /// <param name="contentType">The content type as relevant to the file system.</param>
        /// <param name="cancellationToken"></param>
        Task UploadAsync(string content, string contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return true if the directory exists already.
        /// </summary>
        /// <returns></returns>
        Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Download the content of the file as a text string.
        /// </summary>
        /// <returns></returns>
        Task<string> DownloadTextAsync(CancellationToken cancellationToken = default);
    }
}
