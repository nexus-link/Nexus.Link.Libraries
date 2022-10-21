using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Model;
using System;
using JetBrains.Annotations;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The object with id <see cref="FromId"/> has been replaced by object with id <see cref="ToId"/>.
    /// </summary>
    /// <example>
    /// Two customers (#1 and #2) are duplicates of the same customer. They have been merged into customer #3.
    /// All requests for customers #1 and #2 should throw this exception that should point to #3 as the <see cref="ToId"/>.
    /// </example>
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
        public FulcrumRedirectException(string fromId, string toId) : this(
            $"The id {fromId} should be replaced with {toId}.", (Exception) null)
        {
            FromId = fromId;
            ToId = toId;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumRedirectException(string message, Exception innerException) : base(message, innerException)
        {
            
            SetProperties(innerException);
            if (innerException is FulcrumException innerFulcrumException) CopyFrom(innerFulcrumException);
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <summary>
        /// The old id that should be replaced with <see cref="ToId"/>.
        /// </summary>
        public string FromId
        {
            get => GetData<string>(nameof(FromId));
            set => SetData(nameof(FromId), value);
        }

        /// <summary>
        /// The new id that should replace <see cref="FromId"/>.
        /// </summary>
        public string ToId
        {
            get => GetData<string>(nameof(ToId));
            set => SetData(nameof(ToId), value);
        }

        /// <inheritdoc />
        public override string FriendlyMessage { get; set; } =
            "The specified object id should be replaced with the given object id.";

        private void SetProperties(Exception innerException = null)
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
