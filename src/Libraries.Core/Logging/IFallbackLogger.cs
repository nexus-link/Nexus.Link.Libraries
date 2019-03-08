using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Interface for a fallback mechanism when normal logging fails.
    /// </summary>
    public interface IFallbackLogger
    {
        /// <summary>
        /// Log <paramref name="message"/> with level <paramref name="logSeverityLevel"/>.
        /// </summary>
        /// <remarks>This method will be called when normal logging fails, so use a real simple logger like <see cref="ConsoleLogger"/> or <see cref="TraceSourceLogger"/></remarks>
        void SafeLog(LogSeverityLevel logSeverityLevel, string message);
    }
}