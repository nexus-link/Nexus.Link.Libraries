using System;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The resource was temporarily locked, please try again in the recommended time span (<see cref="FulcrumException.RecommendedWaitTimeInSeconds"/>).
    /// </summary>
    public class FulcrumResourceLockedException : FulcrumTryAgainException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public new static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumResourceLockedException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>.
        /// </summary>
        public new const string ExceptionType = "Xlent.Fulcrum.ResourceLocked";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceLockedException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceLockedException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumResourceLockedException(string message, Exception innerException) : base(message, innerException)
        {
            SetProperties();
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = true;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        private void SetProperties()
        {
            if (RecommendedWaitTimeInSeconds <= 0.0) RecommendedWaitTimeInSeconds = 2;

            FriendlyMessage =
                "The resource was temporarily locked, please try again.";

            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}