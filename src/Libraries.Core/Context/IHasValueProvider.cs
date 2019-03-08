using System;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Tells that the implementor has a public <see cref="IValueProvider"/> property.
    /// </summary>
    [Obsolete("This interface is not in use anymore.")]
    public interface IHasValueProvider
    {
        /// <summary>
        /// The value provider that is used to getting and setting data
        /// </summary>
        IValueProvider ValueProvider { get; }
    }
}