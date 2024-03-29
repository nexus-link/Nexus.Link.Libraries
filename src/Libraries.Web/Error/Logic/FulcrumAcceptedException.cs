﻿using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// The server has accepted the request and will execute it in the background. 
    /// </summary>
    [Obsolete("Use RequestAcceptedException. Obsolete since 2021-10-05.")]
    public class FulcrumAcceptedException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException)
        {
            FulcrumAssert.IsNull(innerException, CodeLocation.AsString());
            return new FulcrumAcceptedException(message);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.Accepted";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumAcceptedException() : this(null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumAcceptedException(string urlWhereResponseWillBeMadeAvailable) : base(urlWhereResponseWillBeMadeAvailable) { }


        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        public string UrlWhereResponseWillBeMadeAvailable => Message;

        /// <inheritdoc />
        public override string FriendlyMessage { get; set; } =
            "The request has been accepted and will eventually be executed. The response can be found at the URL in the error message.";

        private void SetProperties(Exception innerException = null)
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }
    }
}
