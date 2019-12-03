using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{
    [Obsolete("Use ManyToOneSql.")]
    public class ForeignKeyTableHandler<TLocalItem, TForeignItem> : TableBase<TLocalItem>
        where TLocalItem : ITableItem, IValidatable
        where TForeignItem : ITableItem, IValidatable
    {
        public string GroupColumnName { get; }
        public TableBase<TForeignItem> ForeignHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName"></param>
        /// <param name="foreignHandler"></param>
        public ForeignKeyTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName, TableBase<TForeignItem> foreignHandler)
            : base(connectionString, tableMetadata)
        {
            GroupColumnName = groupColumnName;
            ForeignHandler = foreignHandler;
        }

        public async Task<PageEnvelope<TForeignItem>> ReadForeignAsync(Guid groupValue, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS local" +
                             $" JOIN [{ForeignHandler.TableName}] AS foregin ON (foreign.Id = local.{GroupColumnName})" +
                             $" WHERE local.[{GroupColumnName}] = @GroupValue";
            return await ForeignHandler.SearchAdvancedAsync("SELECT COUNT(foreign.[Id])", "SELECT foreign.*", selectRest, TableMetadata.GetOrderBy("local"), new { GroupValue = groupValue }, offset, limit, token);
        }
    }
}
