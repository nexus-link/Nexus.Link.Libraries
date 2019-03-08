using System;

namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Properties for a data "record" that has timestamps for creation and updates.
    /// </summary>
    public interface ITimeStamped
    {
        /// <summary>
        /// The time when a "record" was created.
        /// </summary>
        DateTimeOffset RecordCreatedAt { get; set; }

        /// <summary>
        /// The time when a "record" was last updated.
        /// </summary>
        DateTimeOffset RecordUpdatedAt { get; set; }
    }
}
