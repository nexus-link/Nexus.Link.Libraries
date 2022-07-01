using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class AnnotatedId<TId> : IUniquelyIdentifiable<TId>
    {
        /// <inheritdoc />
        public TId Id { get; set; }
        public string Title { get; set; }
    }
}