using System;
using System.Collections.Generic;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Interface for accessing data.
    /// </summary>
    public interface IContextValueProvider : IValueProvider
    {
        /// <summary>
        /// A unique id for a specific context.
        /// </summary>
        Guid ContextId { get; }

        /// <summary>
        /// Reset the data back to no data stored.
        /// </summary>
        void Reset();

        /// <summary>
        /// Reset the data back to a point that was saved earlier.
        /// </summary>
        /// <param name="dictionary"> The data that was saved by using <see cref="SaveContext"/>.</param>
        void RestoreContext(IDictionary<string, object> dictionary);

        /// <summary>
        /// Get all the current data that the value provider knows of.
        /// </summary>
        /// <returns>An internal representation of data.</returns>
        /// <remarks> Typically used as parameter to <see cref="RestoreContext"/>.</remarks>
        IDictionary<string, object> SaveContext();
    }
}