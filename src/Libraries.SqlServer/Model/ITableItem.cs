using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statmements
    /// </summary>
    public interface ITableItem : IRecommendedStorableItem<Guid>, IOptimisticConcurrencyControlByETag
    {
    }
}