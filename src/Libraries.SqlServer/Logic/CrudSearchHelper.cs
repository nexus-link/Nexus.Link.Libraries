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
                .Select(key => $"[{key}] {EqualsOrLike(key, details)} @{key}")
                .ToList();
            comparisons.AddRange(extraColumnNames.Select(columnName => $"[{columnName}] = @{columnName}"));

            var where = string.Join(" AND ", comparisons);

            return string.IsNullOrWhiteSpace(@where) ? null : @where;
        }

        private static string EqualsOrLike<T>(string key, SearchDetails<T> details)
        {
            var whereCondition = details.GetWhereCondition(key, "%", "_");
            return whereCondition.IsWildCard ? "LIKE" : "=";
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