﻿namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Contains a byte array
    /// </summary>
    public interface IStorableByteArray<TId> : IUniquelyIdentifiable<TId>
    {
        /// <summary>
        /// The content as a byte array
        /// </summary>
        byte[] ByteArray { get; set; }
    }
}