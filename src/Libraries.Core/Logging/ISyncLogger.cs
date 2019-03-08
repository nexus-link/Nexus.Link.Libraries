namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Interface for synchronous logging
    /// </summary>
    public interface ISyncLogger
    {
        /// <summary>
        /// Synchronously log one <paramref name="logRecord"/>.
        /// </summary>
        void LogSync(LogRecord logRecord);
    }
}