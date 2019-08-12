namespace Nexus.Link.Services.Contracts.Events
{
    public class PublishableEvent : IPublishableEvent
    {
        /// <inheritdoc />
        public EventMetadata Metadata { get; set; }
    }
}