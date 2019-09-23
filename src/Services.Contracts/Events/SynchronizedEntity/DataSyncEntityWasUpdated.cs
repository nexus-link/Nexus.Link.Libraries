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
