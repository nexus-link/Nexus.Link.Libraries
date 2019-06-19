namespace Nexus.Link.Libraries.Core.Decoupling
{
    /// <inheritdoc cref="INamedSchema" />
    public class NamedSchema : AnonymousSchema, INamedSchema
    {
        /// <inheritdoc />
        public string SchemaName { get; set; }
    }
}