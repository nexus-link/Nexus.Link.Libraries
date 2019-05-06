namespace Nexus.Link.Libraries.Core.Decoupling
{
    /// <inheritdoc />
    public class NamedSchema : INamedSchema
    {
        /// <inheritdoc />
        public int? SchemaVersion { get; set; }

        /// <inheritdoc />
        public string SchemaName { get; set; }
    }
}