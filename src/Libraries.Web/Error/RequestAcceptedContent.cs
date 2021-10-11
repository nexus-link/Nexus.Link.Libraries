using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.Error
{
    public class RequestAcceptedContent : IValidatable
    {
        /// <summary>
        /// The Url where the response will be made available once it has completed.
        /// </summary>
        public string UrlWhereResponseWillBeMadeAvailable { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(UrlWhereResponseWillBeMadeAvailable, nameof(UrlWhereResponseWillBeMadeAvailable), errorLocation);
        }
    }
}