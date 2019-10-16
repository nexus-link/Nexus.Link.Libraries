using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Minimum data for a publishable event
    /// </summary>
    public interface IPublishableEvent : IValidatable
    {
        EventMetadata Metadata { get; }
    }
}