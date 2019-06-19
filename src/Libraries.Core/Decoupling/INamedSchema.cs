using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Link.Libraries.Core.Decoupling
{
    /// <summary>
    /// This interface is typically used when more than one type of data is stored at the same place and therefore the type of data needs to be stored together with the data.
    /// By first reading the data into a <see cref="T:Nexus.Link.Libraries.Core.Decoupling.NamedSchema" />, the type and version can be determined and the data can be read into
    /// the correct type with the correct version of that type.
    /// </summary>
    /// <inheritdoc />
    public interface INamedSchema : IVersionedSchema
    {
        /// <summary>
        /// The name of the schema for the data.
        /// </summary>
        string SchemaName { get; }
    }
}
