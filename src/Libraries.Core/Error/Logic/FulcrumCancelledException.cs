using System;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The server has encountered a situation it does not know how to handle.
    /// </summary>
    /// <example>
    /// A workflow has been cancelled by an administrator.
    /// </example>
    public class FulcrumCancelledException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumResourceException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.Cancelled";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumCancelledException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumCancelledException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumCancelledException(string message, Exception innerException) : base(message, innerException)
        {
            
            SetProperties(innerException);
            if (innerException is FulcrumException innerFulcrumException) CopyFrom(innerFulcrumException);
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <inheritdoc />
        public override string FriendlyMessage { get; set; } = "The server has encountered a situation it does not know how to handle.";

        private void SetProperties(Exception innerException = null)
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
