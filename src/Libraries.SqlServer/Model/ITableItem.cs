using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statements
    /// </summary>
    [Obsolete("Please use Nexus.Link.Libraries.Core.Storage.Model.ITableItem. Obsolete since 2022-04-07")]
    public interface ITableItem : IRecommendedStorableItem<Guid>, IOptimisticConcurrencyControlByETag, Core.Storage.Model.ITableItem
    {
    }
}