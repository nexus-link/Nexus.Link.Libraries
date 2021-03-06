﻿using System;

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
    [Obsolete("Use FulcrumResourceException. Obsolete warning since 2020-06-09, error since 2021-06-09.", true)]
    public class FulcrumResourceContractException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumResourceContractException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.ResourceContract";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceContractException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceContractException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceContractException(string message, Exception innerException) : base(message, innerException)
        {
            SetProperties();
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <inheritdoc />
        public override string FriendlyMessage {get; set;} =
            "A resource used by the application did not seem to behave according to the contract that has been set up.";

        private void SetProperties()
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
