using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statements
    /// </summary>
    public interface ISqlTableMetadata : IValidatable
    {
        /// <summary>
        /// The name of the database table that will hold this type of items.
        /// </summary>
        /// <remarks>Will be surrounded with dbo.[{TableName}}, i.e. just specify the name, no brackets needed.</remarks>
        string TableName { get; }

        string EtagColumnName { get; }

        /// <summary>
        /// The name of column that has the foreign key or null.
        /// </summary>
        string ForeignKeyColumnName { get; }

        /// <summary>
        /// The name of column that has the time stamp for when the record was created.
        /// </summary>
        string CreatedAtColumnName { get; }

        /// <summary>
        /// The name of column that has the time stamp for when the record was last updated.
        /// </summary>
        string UpdatedAtColumnName { get; }

        /// <summary>
        /// The list of columns that you have added, not including the columns in <see cref="IStorableItem{TId}"/> or in <see cref="ITimeStamped"/>.
        /// </summary>
        IEnumerable<string> CustomColumnNames { get; }

        /// <summary>
        /// The name of columns that we should order the rows by.
        /// </summary>
        IEnumerable<string> OrderBy { get; }

        /// <summary>
        /// Will be used as "ORDER BY {OrderBy}" when selecting multiple rows from the table.
        /// </summary>
        string GetOrderBy(string columnPrefix = null);
    }
}