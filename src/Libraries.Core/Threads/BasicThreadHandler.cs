using System;
using System.Threading;

namespace Nexus.Link.Libraries.Core.Threads
{
    /// <summary>
    /// A basic thread handler based on well known concepts.
    /// </summary>
    public class BasicThreadHandler : IThreadHandler
    {
        /// <inheritdoc />
        public Thread FireAndForget(Action<CancellationToken> action, CancellationToken token = default(CancellationToken))
        {
            var thread = new Thread(() => action(token));
            thread.Start();
            return thread;
        }
    }
}