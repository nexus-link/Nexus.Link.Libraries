using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Link.Libraries.Core.Decoupling
{
    /// <summary>
    /// This interface is typically used for data that needs to be stored with different versions of the schema that is stored.
    /// By first reading the data into a <see cref="AnonymousSchema"/>, the version can be determined and the data can be read into
    /// a type that supports the version.
    /// </summary>
    public interface IVersionedSchema
    {
        /// <summary>
        /// The version of a schema
        /// </summary>
        int? SchemaVersion { get; }
    }
}
