using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Assert
{
    /// <summary>
    /// A generic class for asserting things that the programmer thinks is true. Generic in the meaning that a parameter says what exception that should be thrown when an assumption is false.
    /// </summary>
    internal static class GenericBase<TException>
        where TException : FulcrumException
    {
        [StackTraceHidden]
        public static void ThrowException(string message, string errorLocation = null)
        {
            var exception = (TException)Activator.CreateInstance(typeof(TException), message);
            exception.ErrorLocation = errorLocation;
            if (exception is FulcrumAssertionFailedException ||
                exception is FulcrumContractException)
            {
                var logMessage = "An unexpected internal error resulted in an exception";
                if (!string.IsNullOrWhiteSpace(errorLocation))
                {
                    logMessage += $" in {errorLocation}";
                }

                logMessage += ".";

                // Special case when there is something wrong in the ApplicationSetup, we do not log,
                // because that would trigger a validation of the ApplicationSetup,
                // which hasn't had a chance to set the properties.
                if (FulcrumApplication.Setup.SynchronousFastLogger != null)
                {
                    Log.LogError(logMessage, exception);
                }
            }
            throw exception;
        }
    }
}
