namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Contains a byte array
    /// </summary>
    public class StorableByteArray<TId> : IStorableByteArray<TId>, IOptimisticConcurrencyControlByETag
    {
        /// <inheritdoc />
        public TId Id { get; set; }

        /// <inheritdoc />
        public byte[] ByteArray { get; set; }

        /// <inheritdoc />
        public string Etag { get; set; }
    }
}