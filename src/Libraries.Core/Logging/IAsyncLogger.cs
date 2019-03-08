using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Interface for asynchronous logging
    /// </summary>
    public interface IAsyncLogger
    {
        /// <summary>
        /// Asynchronously log one <paramref name="logRecord"/>.
        /// </summary>
        Task LogAsync(LogRecord logRecord);
    }
}