using System;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The server failed to execute the request due to a resource not behaving according to the contract.
    /// </summary>
    /// <example>
    /// We call an external service, expecting it to either be successful, or to return a FulcrumError. If it doesn't, this kind of exception is thrown.
    /// </example>
    /// <remarks>
    /// This exception is a way to blame someone else for a problem that has occurred in your code.
    /// </remarks>
    public class FulcrumResourceErrorException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumResourceErrorException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.ResourceError";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceErrorException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceErrorException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceErrorException(string message, Exception innerException) : base(message, innerException)
        {
            SetProperties();
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful => false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <inheritdoc />
        public override string FriendlyMessage =>
            "A resource used by the application had an internal error."
            + "\rPlease report the following:"
            + $"\rCorrelationId: {CorrelationId}"
            + $"\rInstanceId: {InstanceId}";

        private void SetProperties()
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
