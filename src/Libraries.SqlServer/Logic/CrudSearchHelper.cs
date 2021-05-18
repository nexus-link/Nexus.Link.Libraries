using System.Linq;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    public static class CrudSearchHelper
    {

        /// <summary>
        /// Constructs a WHERE statement from search details
        /// </summary>
        public static string GetWhereStatement<T>(SearchDetails<T> details, params string [] extraColumnNames)
        {
            if (!details.WhereAsSortedDictionary.Any() && extraColumnNames == null) return null;
            var comparisons = details.WhereAsSortedDictionary.Keys
                .Select(key => $"[{key}] = @{key}")
                .ToList();
            comparisons.AddRange(extraColumnNames.Select(columnName => $"[{columnName}] = @{columnName}"));

            var where = string.Join(" AND ", comparisons);

            return where;
        }

        /// <summary>
        /// Constructs an ORDER BY statement from search details
        /// </summary>
        public static string GetOrderByStatement<T>(SearchDetails<T> details)
        {
            if (!details.OrderByAsSortedDictionary.Any()) return null;
            var orderItems = details.OrderByAsSortedDictionary.Select(pair => $"[{pair.Key}] {AscOrDesc(pair.Value)}");
            var orderBy = string.Join(", ", orderItems);

            return orderBy;

            
        }

        private static string AscOrDesc(bool ascending) => ascending ? "ASC" : "DESC";
    }
}