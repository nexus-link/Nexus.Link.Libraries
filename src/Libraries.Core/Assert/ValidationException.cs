using System;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Assert
{
    /// <summary>
    /// The base class for all Fulcrum exceptions
    /// </summary>
    public class ValidationException : FulcrumException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
