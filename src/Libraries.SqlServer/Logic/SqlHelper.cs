using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    internal static class SqlHelper
    {
        public static string Create(ISqlTableMetadata tableMetadata) => $"INSERT INTO dbo.[{tableMetadata.TableName}] ({CreateColumnList(tableMetadata)}) VALUES ({CreateArgumentList(tableMetadata)})";
        public static string CreateAndRead(ISqlTableMetadata tableMetadata) => $"INSERT INTO dbo.[{tableMetadata.TableName}] ({CreateColumnList(tableMetadata)}) OUTPUT {ReadColumnList(tableMetadata, "INSERTED")} VALUES ({CreateArgumentList(tableMetadata)})";

        public static string Read(ISqlTableMetadata tableMetadata, string where) => $"SELECT {ReadColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where}";

        public static string Read(ISqlTableMetadata tableMetadata, string where, string orderBy) => $"SELECT {ReadColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where} ORDER BY {orderBy}";

        [Obsolete("Please use UpdateIfSameEtag(). Obsolete warning since 2020-01-12, error since 2021-06-09.", true)]
        public static string Update(ISqlTableMetadata tableMetadata, string oldEtag) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id AND Etag = '{oldEtag}'";

        public static string UpdateIfSameEtag(ISqlTableMetadata tableMetadata, string oldEtag) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id AND {tableMetadata.EtagColumnName} = '{oldEtag}'";
        public static string UpdateIfSameEtagAndRead(ISqlTableMetadata tableMetadata, string oldEtag) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} OUTPUT {ReadColumnList(tableMetadata, "INSERTED")} WHERE Id = @Id AND {tableMetadata.EtagColumnName} = '{oldEtag}'";

        public static string UpdateIfSameRowVersion(ISqlTableMetadata tableMetadata) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id AND {tableMetadata.RowVersionColumnName} = @{tableMetadata.RowVersionColumnName}";
        public static string UpdateIfSameRowVersionAndRead(ISqlTableMetadata tableMetadata) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} OUTPUT {ReadColumnList(tableMetadata, "INSERTED")} WHERE Id = @Id AND {tableMetadata.RowVersionColumnName} = @{tableMetadata.RowVersionColumnName}";

        public static string Update(ISqlTableMetadata tableMetadata) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id";
        public static string UpdateAndRead(ISqlTableMetadata tableMetadata) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} OUTPUT {ReadColumnList(tableMetadata, "INSERTED")} WHERE Id = @Id";

        public static string Delete(ISqlTableMetadata tableMetadata, string where) => $"DELETE FROM [{tableMetadata.TableName}] WHERE {where}"; 
        
        [Obsolete("Please use CreateColumnList or ReadColumnList(). Obsolete warning since 2020-01-12, error since 2021-06-09.", true)]
        public static string ColumnList(ISqlTableMetadata tableMetadata) => string.Join(", ", AllColumnNames(tableMetadata).Select(name => $"[{name}]"));

        public static string CreateColumnList(ISqlTableMetadata tableMetadata) => string.Join(", ", CreateColumnNames(tableMetadata).Select(name => $"[{name}]"));

        public static string ReadColumnList(ISqlTableMetadata tableMetadata, string alias) => string.Join(", ", ReadColumnNames(tableMetadata).Select(name => $"{alias}.[{name}]"));
        public static string ReadColumnList(ISqlTableMetadata tableMetadata) => string.Join(", ", ReadColumnNames(tableMetadata).Select(name => $"[{name}]"));

        [Obsolete("Please use CreateArgumentList(). Obsolete warning since 2020-01-12., error since 2021-06-09.", true)]
        public static string ArgumentList(ISqlTableMetadata tableMetadata) => string.Join(", ", AllColumnNames(tableMetadata).Select(name => $"@{name}"));

        public static string CreateArgumentList(ISqlTableMetadata tableMetadata) => string.Join(", ", CreateColumnNames(tableMetadata).Select(name => $"@{name}"));

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
            if (tableMetadata.EtagColumnName != null) list.Add($"{tableMetadata.EtagColumnName}='{StorageHelper.CreateNewId<Guid>()}'");
            if (tableMetadata.UpdatedAtColumnName != null) list.Add($"{tableMetadata.UpdatedAtColumnName}=sysutcdatetime()");
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

        public static IEnumerable<string> ReadColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string> { "Id"};
            if (tableMetadata.EtagColumnName != null) list.Add(tableMetadata.EtagColumnName);
            if (tableMetadata.RowVersionColumnName != null) list.Add(tableMetadata.RowVersionColumnName);
            if (tableMetadata.CreatedAtColumnName != null) list.Add(tableMetadata.CreatedAtColumnName);
            if (tableMetadata.UpdatedAtColumnName != null) list.Add(tableMetadata.UpdatedAtColumnName);
            list.AddRange(tableMetadata.CustomColumnNames);
            return list;
        }

        public static IEnumerable<string> CreateColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string> { "Id"};
            if (tableMetadata.EtagColumnName != null) list.Add(tableMetadata.EtagColumnName);
            list.AddRange(tableMetadata.CustomColumnNames);
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
