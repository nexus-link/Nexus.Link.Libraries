using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Model;
using System;
using JetBrains.Annotations;

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
    public class FulcrumRedirectException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumRedirectException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.Redirect";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumRedirectException() : this(null, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumRedirectException(string message) : this(message, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumRedirectException(string oldId, string newId) : this(
            $"The id {oldId} should be replaced with {newId}.", (Exception) null)
        {
            OldId = oldId;
            NewId = newId;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumRedirectException(string message, Exception innerException) : base(message, innerException)
        {
            
            SetProperties(innerException);
            if (innerException is FulcrumException innerFulcrumException) InternalCopyFrom(innerFulcrumException);
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <summary>
        /// The old id that should be replaced with <see cref="NewId"/>.
        /// </summary>
        public string OldId
        {
            get => Data.Contains(nameof(OldId)) ? (string) Data[nameof(OldId)] : null;
            set => Data[nameof(OldId)] = value;
        }

        /// <summary>
        /// The new id that should replace <see cref="OldId"/>.
        /// </summary>
        public string NewId
        {
            get => Data.Contains(nameof(NewId)) ? (string)Data[nameof(NewId)] : null;
            set => Data[nameof(NewId)] = value;
        }

        /// <inheritdoc />
        public override string FriendlyMessage { get; set; } =
            "The specified object id should be replaced with the given object id.";

        private FulcrumRedirectException InternalCopyFrom(IFulcrumError fulcrumError)
        {
            base.CopyFrom(fulcrumError);
            return this;
        }

        public override IFulcrumError CopyFrom(IFulcrumError fulcrumError)
        {
            return InternalCopyFrom(fulcrumError);
        }

        private void SetProperties(Exception innerException = null)
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
