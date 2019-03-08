using System;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Interface for accessing a CorrelationId.
    /// </summary>
    [Obsolete("Use CorrelationIdValueProvider directly.", true)]
    public interface ICorrelationIdValueProvider
    {

        /// <summary>
        /// Access method for CorrelationId
        /// </summary>
        string CorrelationId { get; set; }
    }
}