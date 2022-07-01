using System;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// An abstract class for all exceptions that are of the type "Programmers Error".
    /// </summary>
    public abstract class FulcrumProgrammersErrorException : FulcrumException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected FulcrumProgrammersErrorException() : this(null, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected FulcrumProgrammersErrorException(string message) : this(message, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected FulcrumProgrammersErrorException(string message, string errorLocation) : this(message, (Exception)null)
        {
            ErrorLocation = errorLocation;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected FulcrumProgrammersErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

    }
}
