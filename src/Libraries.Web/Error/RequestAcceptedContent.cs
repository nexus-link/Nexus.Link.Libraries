using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.Error
{
    public class RequestAcceptedContent : IValidatable
    {
        /// <summary>
        /// The id that represents this request.
        /// </summary>
        public string RequestId { get; set; }
        
        /// <summary>
        /// The Url where the response will be made available once it has completed.
        /// </summary>
        public string PollingUrl { get; set; }

        /// <summary>
        /// The Url where the you can register for a callback when the response is available.
        /// </summary>
        public string RegisterCallbackUrl { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(RequestId, nameof(RequestId), errorLocation);
        }
    }
}