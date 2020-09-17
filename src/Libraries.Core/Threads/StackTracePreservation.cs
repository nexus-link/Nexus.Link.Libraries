using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Core.Threads
{
    /// <summary>
    /// A way to preserve important properties when starting new threads/jobs.
    /// </summary>
    public class StackTracePreservation : ILoggable
    {
        private const int MaxDepthForBackgroundThreads = 5;
        private readonly List<string> _stackTraces;
        private readonly int _callDepth;

        private static readonly AsyncLocal<int> ThreadCallDepth = new AsyncLocal<int> {Value = 0};
        private static readonly AsyncLocal<List<string>> ThreadStackTraces = new AsyncLocal<List<string>> {Value = null};

        private static readonly object ClassLock = new object();

        /// <summary></summary>
        public StackTracePreservation()
        {
            lock (ClassLock)
            {
                _callDepth = ThreadCallDepth.Value;
                _stackTraces = new List<string>
                {
                    Environment.StackTrace
                };
                if (ThreadStackTraces.Value != null) _stackTraces.AddRange(ThreadStackTraces.Value);
            }
        }

        /// <summary>
        /// Restore the stack trace, execute the action. Never throws an exception.
        /// </summary>
        /// <param name="action">The action to run in the background.</param>
        public void ExecuteActionFailSafe(Action action)
        {
            try
            {
                RestoreStackTrace();
                action();
                lock (ClassLock)
                {
                    if (ThreadStackTraces.Value.Count != 0) ThreadStackTraces.Value.RemoveAt(ThreadStackTraces.Value.Count - 1);
                    ThreadCallDepth.Value--;
                }
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Critical, "Background thread failed.", e);
            }
        }

        /// <summary>
        /// Restore the stack trace, execute the action. Never throws an exception.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        public async Task ExecuteActionFailSafeAsync(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            try
            {
                RestoreStackTrace();
                await asyncMethod(token);
                lock (ClassLock)
                {
                    if (ThreadStackTraces.Value.Count != 0) ThreadStackTraces.Value.RemoveAt(ThreadStackTraces.Value.Count - 1);
                    ThreadCallDepth.Value--;
                }
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Critical, "Background thread failed.", e);
            }
        }

        /// <summary>
        /// Restore the stack trace, execute the action. Never throws an exception.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        public async Task ExecuteActionFailSafeAsync(Func<Task> asyncMethod)
        {
            try
            {
                RestoreStackTrace();
                await asyncMethod();
                lock (ClassLock)
                {
                    if (ThreadStackTraces.Value.Count != 0) ThreadStackTraces.Value.RemoveAt(ThreadStackTraces.Value.Count - 1);
                    ThreadCallDepth.Value--;
                }
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Critical, "Background thread failed.", e);
            }
        }

        private void RestoreStackTrace()
        {
            lock (ClassLock)
            {
                ThreadStackTraces.Value = _stackTraces;
                ThreadCallDepth.Value = _callDepth + 1;
            }
            FulcrumAssert.IsLessThan(MaxDepthForBackgroundThreads, ThreadCallDepth.Value, null, "Too deep nesting of background jobs.");
        }

        /// <inheritdoc />
        public string ToLogString()
        {
            var message = "";
            lock (ClassLock)
            {
                if (ThreadStackTraces.Value == null) return message;
                message = ThreadStackTraces.Value
                    .Aggregate(message, (current, stackTrace) => current + $"\r\r{stackTrace}");
            }
            return message;
        }
    }
}
