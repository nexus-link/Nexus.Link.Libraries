using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Core.Threads
{
    /// <summary>
    /// Convenience for choosing the right <see cref="IThreadHandler"/>.
    /// </summary>
    public static class ThreadHelper
    {

        /// <summary>
        /// Execute an <paramref name="action"/> in the background.
        /// </summary>
        /// <param name="action">The action to run in the background.</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForget(Action action)
        {
            FulcrumApplication.ValidateButNotInProduction();
            var messageIfException = $"Background thread failed.";
            return FulcrumApplication.Setup.ThreadHandler.FireAndForget(ct => ExecuteActionFailSafe(action, messageIfException));
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForget(Func<Task> asyncMethod)
        {
            FulcrumApplication.ValidateButNotInProduction();
            var messageIfException = $"Background thread failed.";
            return FulcrumApplication.Setup.ThreadHandler.FireAndForget(ct => CallAsyncFromSync(asyncMethod, messageIfException, ct));
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForget(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            FulcrumApplication.ValidateButNotInProduction();
            var messageIfException = $"Background thread failed.";
            return FulcrumApplication.Setup.ThreadHandler.FireAndForget(ct => CallAsyncFromSync(asyncMethod, messageIfException, ct), token);
        }

        /// <summary>
        /// Execute an <paramref name="action"/> in the background.
        /// </summary>
        /// <param name="action">The action to run in the background.</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForgetResetContext(Action action)
        {
            FulcrumApplication.ValidateButNotInProduction();
            return FireAndForget(() => ResetBeforeCall(action));
        }

        private static void ResetBeforeCall(Action action)
        {
            FulcrumApplication.Context.ValueProvider.Reset();
            action();
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForgetResetContext(Func<Task> asyncMethod)
        {
            FulcrumApplication.ValidateButNotInProduction();
            return FireAndForget(async () => await ResetBeforeCall(asyncMethod));
        }

        private static Task ResetBeforeCall(Func<Task> asyncMethod)
        {
            FulcrumApplication.Context.ValueProvider.Reset();
            return asyncMethod();
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForgetResetContext(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            FulcrumApplication.ValidateButNotInProduction();
            return FireAndForget(async () => await ResetBeforeCall(asyncMethod, token));
        }

        private static Task ResetBeforeCall(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            FulcrumApplication.Context.ValueProvider.Reset();
            return asyncMethod(token);
        }

        /// <summary>
        /// Execute an <paramref name="action"/> in the background.
        /// </summary>
        /// <param name="action">The action to run in the background.</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForgetWithExpensiveStackTracePreservation(Action action)
        {
            FulcrumApplication.ValidateButNotInProduction();
            var context = new StackTracePreservation();
            return FireAndForget(() => context.ExecuteActionFailSafe(action));
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForgetWithExpensiveStackTracePreservation(Func<Task> asyncMethod)
        {
            FulcrumApplication.ValidateButNotInProduction();
            var context = new StackTracePreservation();
            return FireAndForget(() => context.ExecuteActionFailSafeAsync(asyncMethod));
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The created thread.</returns>
        public static Thread FireAndForgetWithExpensiveStackTracePreservation(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            FulcrumApplication.ValidateButNotInProduction();
            var context = new StackTracePreservation();
            return FireAndForget(ct => context.ExecuteActionFailSafeAsync(asyncMethod,ct), token);
        }

        /// <summary>
        /// Restore the context, execute the action. Never throws an exception.
        /// </summary>
        /// <param name="action">The action to run in the background.</param>
        /// <param name="messageIfException">The message to display if there was an exception</param>
        public static void ExecuteActionFailSafe(Action action, string messageIfException)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Error, messageIfException, e);
            }
        }

        /// <summary>
        /// Restore the context, execute the action. Never throws an exception.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="messageIfException">The message to display if there was an exception</param>
        public static async Task ExecuteActionFailSafeAsync(Func<Task> asyncMethod, string messageIfException)
        {
            try
            {
                await asyncMethod();
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Error, messageIfException, e);
            }
        }

        /// <summary>
        /// Restore the context, execute the action. Never throws an exception.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="messageIfException">The message to display if there was an exception</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        public static async Task ExecuteActionFailSafeAsync(Func<CancellationToken, Task> asyncMethod, string messageIfException, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await asyncMethod(token);
            }
            catch (Exception e)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Error, messageIfException, e);
            }
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        public static void CallAsyncFromSync(Func<CancellationToken, Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            // This way to call an async method from a synchronous method was found here:
            // https://stackoverflow.com/questions/40324300/calling-async-methods-from-non-async-code
            Task.Run(async () => await asyncMethod(token), token).Wait(token);
        }

        /// <summary>
        /// Execute an <paramref name="asyncMethod"/> in the background.
        /// </summary>
        /// <param name="asyncMethod">The action to run in the background.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        public static void CallAsyncFromSync(Func<Task> asyncMethod, CancellationToken token = default(CancellationToken))
        {
            // This way to call an async method from a synchronous method was found here:
            // https://stackoverflow.com/questions/40324300/calling-async-methods-from-non-async-code
            Task.Run(asyncMethod, token).Wait(token);
        }

        private static void CallAsyncFromSync(Func<CancellationToken, Task> asyncMethod, string messageIfException, CancellationToken token = default(CancellationToken))
        {
            CallAsyncFromSync(async ct => await ExecuteActionFailSafeAsync(asyncMethod, messageIfException, ct), token);
        }

        private static void CallAsyncFromSync(Func<Task> asyncMethod, string messageIfException, CancellationToken cancellationToken)
        {
            CallAsyncFromSync(async () => await ExecuteActionFailSafeAsync(asyncMethod, messageIfException), cancellationToken);
        }

        /// <summary>
        /// Default <see cref="IContextValueProvider"/> for .NET Framework.
        /// </summary>
        public static IThreadHandler RecommendedForRuntime { get; } = new BasicThreadHandler();

    }
}
