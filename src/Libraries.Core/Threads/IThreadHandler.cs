using System;
using System.Threading;

namespace Nexus.Link.Libraries.Core.Threads
{
    /// <summary>
    /// Required functionality for a thread handler
    /// </summary>
    public interface IThreadHandler
    {
        /// <summary>
        /// Execute an <paramref name="action"/> in the background.
        /// </summary>
        /// <param name="action">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The created thread.</returns>
        Thread FireAndForget(Action<CancellationToken> action, CancellationToken token = default);
    }
}