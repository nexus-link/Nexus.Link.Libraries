using System;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The programmer has called a method in a way that breaks the contract.
    /// </summary>
    /// <remarks>
    /// This is basically a "Programmers Error", a bug on the server side.
    /// </remarks>
    public class FulcrumContractException : FulcrumProgrammersErrorException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumContractException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>.
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.Contract";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException() : this(null, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException(string message) : this(message, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException(string message, string errorLocation) : this(message, (Exception) null)
        {
            ErrorLocation = errorLocation;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumContractException(string message, Exception innerException) : base(message, innerException)
        {
            SetProperties();
        }

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <inheritdoc />
        public override string FriendlyMessage {get; set;} =
            "A programmer's code calls another part of the program in a bad way. An end user is never supposed to see this error as it should be converted on the way.";

        private void SetProperties()
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
