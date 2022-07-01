using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

#pragma warning disable 659

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A  uniquely identifiable item that implements <see cref="IOptimisticConcurrencyControlByETag"/> to be used in testing
    /// </summary>
    public partial class TestItemEtag<TId> : TestItemId<TId>, IOptimisticConcurrencyControlByETag, IValidatable
    {
        /// <inheritdoc />
        public string Etag { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
        }
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
