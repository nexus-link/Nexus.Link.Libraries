using System;

namespace Nexus.Link.Services.Contracts.Events.SynchronizedEntity
{
    /// <summary>
    /// This event is published whenever a new Invoices has been published.
    /// </summary>
    public class DataSyncEntityWasUpdated : IPublishableEvent
    {
        /// <inheritdoc />
        public EventMetadata Metadata { get; set; } = 
            new EventMetadata("DataSyncEntity", "Updated", 1, 0);

        public SyncKey Key { get; } = new SyncKey();

        /// <summary>
        /// The time (ISO8061) when the object was updated.
        /// </summary>
        public string TimeStamp { get; } = DateTime.UtcNow.ToString("o");

        /// <summary>
        /// Optional. Name of the user that caused the update.
        /// </summary>
        public string UserName { get; set; }
    }

    public class SyncKey
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
    }
}
