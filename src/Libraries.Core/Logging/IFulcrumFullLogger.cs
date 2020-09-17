using System;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Interface for basic logging
    /// </summary>
    [Obsolete("Use ISyncLogger or IAsyncLogger", true)]
    public interface IFulcrumFullLogger : IAsyncLogger
    {
        /// <summary>
        /// Log <paramref name="message"/> with level <paramref name="logSeverityLevel"/>.
        /// </summary>
        void Log(LogSeverityLevel logSeverityLevel, string message);
    }
}