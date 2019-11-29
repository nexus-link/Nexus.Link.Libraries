using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Platform.DataSyncEngine
{
    public class SyncKey : IValidatable
    {
        /// <summary>
        /// The name of the sync client
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// The name of the entity.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// The id of the object that has been updated.
        /// </summary>
        public string Value { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(ClientName, nameof(ClientName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(EntityName, nameof(EntityName), errorLocation);
            // Value can be null when creating objects
        }
    }
}