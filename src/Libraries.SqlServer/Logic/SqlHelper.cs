using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    internal static class SqlHelper
    {
        public static string Create(ISqlTableMetadata tableMetadata) => $"INSERT INTO dbo.[{tableMetadata.TableName}] ({ColumnList(tableMetadata)}) values ({ArgumentList(tableMetadata)})";

        public static string Read(ISqlTableMetadata tableMetadata, string where) => $"SELECT {ColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where}";

        public static string Read(ISqlTableMetadata tableMetadata, string where, string orderBy) => $"SELECT {ColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where} ORDER BY {orderBy}";

        public static string Update(ISqlTableMetadata tableMetadata, string oldEtag) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id AND Etag = '{oldEtag}'";

        public static string Update(ISqlTableMetadata tableMetadata) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id";

        public static string Delete(ISqlTableMetadata tableMetadata, string where) => $"DELETE FROM [{tableMetadata.TableName}] WHERE {where}";

        public static string ColumnList(ISqlTableMetadata tableMetadata) => string.Join(", ", AllColumnNames(tableMetadata).Select(name => $"[{name}]"));

        public static string ArgumentList(ISqlTableMetadata tableMetadata) => string.Join(", ", AllColumnNames(tableMetadata).Select(name => $"@{name}"));

        public static string UpdateList(ISqlTableMetadata tableMetadata)
        {
            var allUpdates = new List<string>();
            allUpdates.AddRange(UpdatesForStandardColumns(tableMetadata));
            allUpdates.AddRange(tableMetadata.CustomColumnNames.Select(name => $"[{name}]=@{name}"));
            return string.Join(",", allUpdates);
        }

        private static IEnumerable<string> UpdatesForStandardColumns(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string>();
            if (tableMetadata.EtagColumnName != null) list.Add($"{tableMetadata.EtagColumnName}='{Guid.NewGuid()}'");
            if (tableMetadata.UpdatedAtColumnName != null) list.Add($"{tableMetadata.UpdatedAtColumnName}='{DateTimeOffset.Now}'");
            return list;
        }

        public static IEnumerable<string> NonCustomColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string> { "Id"};
            if (tableMetadata.EtagColumnName != null) list.Add(tableMetadata.EtagColumnName);
            if (tableMetadata.CreatedAtColumnName != null) list.Add(tableMetadata.CreatedAtColumnName);
            if (tableMetadata.UpdatedAtColumnName != null) list.Add(tableMetadata.UpdatedAtColumnName);
            return list;
        }

        public static IEnumerable<string> AllColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = NonCustomColumnNames(tableMetadata).ToList();
            list.AddRange(tableMetadata.CustomColumnNames);
            return list;
        }
    }
}
