using System;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    /// <summary>
    /// A base class that has the mandatory properties.
    /// </summary>
    /// <remarks>
    /// We recommend to inherit from <see cref="TimeStampedTableItem"/>.
    /// </remarks>
#pragma warning disable CS0618
    public abstract class TableItem : ITableItem, Core.Storage.Model.ITableItem
#pragma warning restore CS0618
    {
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public string Etag { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocaction, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(Id, nameof(Id), errorLocaction);
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocaction);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TableItem o && Id.Equals(o.Id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
        }
    }
}
