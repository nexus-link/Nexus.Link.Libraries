using System.Linq;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    public static class CrudSearchHelper
    {

        /// <summary>
        /// Constructs a WHERE statement from search details
        /// </summary>
        public static string GetWhereStatement<T>(SearchDetails<T> details, params string[] extraColumnNames)
        {
            if (!details.WherePropertyNames.Any() && extraColumnNames == null) return null;
            var comparisons = details.WherePropertyNames
                .Select(key => Comparison(details, key))
                .ToList();
            comparisons.AddRange(extraColumnNames.Select(columnName => $"[{columnName}] = @{columnName}"));

            var where = string.Join(" AND ", comparisons);

            return string.IsNullOrWhiteSpace(@where) ? null : @where;
        }

        private static string Comparison<T>(SearchDetails<T> details, string columnName)
        {
            var whereCondition = details.GetWhereCondition(columnName, "%", "_");
            if (whereCondition.Object == null) return $"[{columnName}] IS NULL";
            return whereCondition.IsWildCard ? $"[{columnName}] LIKE @{columnName}" : $"[{columnName}] = @{columnName}";
        }

        /// <summary>
        /// Constructs an ORDER BY statement from search details
        /// </summary>
        public static string GetOrderByStatement<T>(SearchDetails<T> details)
        {
            if (!details.OrderByPropertyNames.Any()) return null;
            var orderItems = details.OrderByPropertyNames.Select(name => $"[{name}] {AscOrDesc(name, details)}");
            var orderBy = string.Join(", ", orderItems);

            return orderBy;


        }

        private static string AscOrDesc<T>(string name, SearchDetails<T> details) => details.IsAscending(name) ? "ASC" : "DESC";
    }
}