using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    public class SqlTableMetadata : ISqlTableMetadata
    {
        /// <inheritdoc />
        public string TableName { get; set; }

        /// <inheritdoc />
        public string EtagColumnName { get; set; }

        /// <inheritdoc />
        public string ForeignKeyColumnName { get; set; }
        /// <inheritdoc />
        public string CreatedAtColumnName { get; set; }
        /// <inheritdoc />
        public string UpdatedAtColumnName { get; set; }
        /// <inheritdoc />
        public IEnumerable<string> CustomColumnNames { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> OrderBy { get; set; }

        /// <inheritdoc />
        public string GetOrderBy(string columnPrefix = null)
        {
            var prefix = string.IsNullOrWhiteSpace(columnPrefix) ? "" : $"{columnPrefix}.";
            var orderBy = string.Join(", ", OrderBy.Select(x => $"{prefix}{x}"));
            return string.IsNullOrWhiteSpace(orderBy) ? null : orderBy;
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(TableName, nameof(TableName), errorLocation);
            FulcrumValidate.IsNotNull(CustomColumnNames, nameof(CustomColumnNames), errorLocation);
            FulcrumValidate.IsNotNull(OrderBy, nameof(OrderBy), errorLocation);
        }
    }
}