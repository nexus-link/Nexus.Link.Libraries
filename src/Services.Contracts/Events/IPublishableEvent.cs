using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Minimum data for a publishable event
    /// </summary>
    public interface IPublishableEvent
    {
        EventMetadata Metadata { get; }
    }
}