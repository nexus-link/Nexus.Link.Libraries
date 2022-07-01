using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A storable item that can be sorted by integer or string
    /// </summary>
    public partial class TestItemSort<TId> : TestItemId<TId>
    {
        public int IncreasingNumber { get; set; }
        public int NumberModulo { get; set; }
        public string DecreasingString { get; set; }
    }

    #region override object
    public partial class TestItemSort<TId>
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is TestItemSort<TId> o)) return false;
            return IncreasingNumber == o.IncreasingNumber && NumberModulo == o.NumberModulo && DecreasingString == o.DecreasingString && base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Id}: {Value} {IncreasingNumber} {DecreasingString}";
        }
    }
    #endregion
}
