using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// Extensions for exceptions
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Use this for calls to services that can throw <see cref="RequestAcceptedException"/>.
        /// If the service accepts the request, the exception is silently ignored.
        /// There is also an option to set <paramref name="ignoreAllExceptions"/>, where all
        /// exceptions are silently ignored.
        /// </summary>
        /// <param name="task">The task that can throw a <see cref="RequestAcceptedException"/></param>
        /// <param name="ignoreAllExceptions">If this is true, then all exceptions are silently ignored.</param>
        public static async Task FireAndForgetAsync(this Task task, bool ignoreAllExceptions = false)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                var requestWasAccepted = (e is RequestAcceptedException);
                if (!requestWasAccepted && !ignoreAllExceptions) throw;
                Log.LogVerbose($"{nameof(FireAndForgetAsync)} silently ignored exception {e}", e);
            }
        }

        /// <summary>
        /// Use this for calls to services that can throw <see cref="RequestAcceptedException"/>.
        /// If the service accepts the request, the exception is silently ignored.
        /// There is also an option to set <paramref name="ignoreAllExceptions"/>, where all
        /// exceptions are silently ignored.
        /// </summary>
        /// <param name="task">The task that can throw a <see cref="RequestAcceptedException"/></param>
        /// <param name="ignoreAllExceptions">If this is true, then all exceptions are silently ignored.</param>
        public static async Task FireAndForgetAsync<T>(this Task<T> task, bool ignoreAllExceptions = false)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                var requestWasAccepted = (e is RequestAcceptedException);
                if (!requestWasAccepted && !ignoreAllExceptions) throw;
                Log.LogVerbose($"{nameof(FireAndForgetAsync)} silently ignored exception {e}", e);
            }
        }
    }
}