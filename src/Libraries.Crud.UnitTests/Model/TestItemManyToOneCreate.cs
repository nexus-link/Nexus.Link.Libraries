using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A minimal storable item that implements <see cref="IUniquelyIdentifiable{TId}"/> to be used in testing
    /// </summary>
    public class TestItemManyToOneCreate<TId, TReferenceId> : TestItemSort<TId>
    {
        public TReferenceId ParentId { get; set; }
    }
}
