using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Core.File
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
        Task UploadAsync(string content, string contentType = null);

        /// <summary>
        /// Return true if the directory exists already.
        /// </summary>
        /// <returns></returns>
        Task<bool> ExistsAsync();

        /// <summary>
        /// Download the content of the file as a text string.
        /// </summary>
        /// <returns></returns>
        Task<string> DownloadTextAsync();
    }
}
