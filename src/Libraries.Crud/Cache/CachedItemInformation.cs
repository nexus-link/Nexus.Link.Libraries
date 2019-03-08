using System;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// Information about a cached item.
    /// </summary>
    /// <typeparam name="TId">THe type for the unique identifier for the item.</typeparam>
    public class CachedItemInformation<TId>
    {
        /// <summary>
        /// The unique identifier for this item.
        /// </summary>
        public TId Id { get; set; }

        /// <summary>
        /// The time that the cached item was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
