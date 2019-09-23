using Nexus.Link.Libraries.Core.Storage.Model;

#pragma warning disable 659

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A  uniquely identifiable item that implements <see cref="IOptimisticConcurrencyControlByETag"/> to be used in testing
    /// </summary>
    public partial class TestItemEtag<TId> : TestItemId<TId>, IOptimisticConcurrencyControlByETag
    {
        /// <inheritdoc />
        public string Etag { get; set; }
    }

    #region override object
    public partial class TestItemEtag<TId>
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is TestItemEtag<TId> o)) return false;
            return Etag.Equals(o.Etag) && base.Equals(obj);
        }
    }
    #endregion
}
