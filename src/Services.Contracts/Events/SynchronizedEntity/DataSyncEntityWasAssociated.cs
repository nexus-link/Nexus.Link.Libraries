using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Services.Contracts.Events.SynchronizedEntity
{
    public class DataSyncEntityWasAssociated : IPublishableEvent
    {
        /// <inheritdoc />
        public EventMetadata Metadata { get; set; } = new EventMetadata("DataSyncEntity", "Associated", 1, 0);

        public List<SyncKey> AssociatedKeys { get; set; }

        /// <summary>
        /// The time (ISO8061) when the object was updated.
        /// </summary>
        public string Timestamp { get; } = DateTime.UtcNow.ToString("o");

        /// <summary>
        /// Optional. Name of the user that caused the update.
        /// </summary>
        public string UserName { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(Metadata, nameof(Metadata), errorLocation);
            FulcrumValidate.IsValidated(Metadata, $"{propertyPath}.{nameof(Metadata)}", nameof(Metadata), errorLocation);
            FulcrumValidate.IsNotNull(AssociatedKeys, nameof(AssociatedKeys), errorLocation);
            var i = 0;
            foreach (var associatedKey in AssociatedKeys)
            {
                FulcrumValidate.IsValidated(associatedKey, $"{propertyPath}[{i++}]", nameof(associatedKey), errorLocation);
            }
        }
    }
}