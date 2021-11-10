using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities
{
    /// <summary>
    /// URL:s that are related to a specific request.
    /// </summary>
    public class RequestResponseEndpoints : IValidatable

    {
        /// <summary>
        /// The endpoint to get a response
        /// </summary>
        public string PollingUrl { get; set; }

        /// <summary>
        /// The endpoint where you can register a callback for the request.
        /// </summary>
        public string RegisterCallbackUrl { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(PollingUrl, nameof(PollingUrl), errorLocation);
        }
    }
}