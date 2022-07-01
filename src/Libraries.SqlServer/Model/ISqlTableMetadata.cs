using System;
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

        /// <summary>
        /// The name of the ETag column, or null
        /// </summary>
        string EtagColumnName { get; }

        /// <summary>
        /// The name of the rowversion column, or null
        /// </summary>
        string RowVersionColumnName { get; }

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
        /// Set this to true if we can use OUTPUT when inserting rows in this table.
        /// </summary>
        /// <remarks>
        /// This makes CreateAndReturn methods atomic and more efficient.
        /// If the table has insert triggers, this must be false.
        /// https://stackoverflow.com/questions/13198476/cannot-use-update-with-output-clause-when-a-trigger-is-on-the-table
        /// </remarks>
        bool InsertCanUseOutput { get; }

        /// <summary>
        /// Set this to true if the table has at least one insert trigger.
        /// If this is true, then we can't use OUTPUT when inserting
        /// https://stackoverflow.com/questions/13198476/cannot-use-update-with-output-clause-when-a-trigger-is-on-the-table
        /// </summary>
        [Obsolete("Use InsertCanUseOutput. HasInsertTrigger = !InsertCanUseOutput. Obsolete since 2022-02-10.")]
        bool HasInsertTrigger { get; }

        /// <summary>
        /// Set this to true if we can use OUTPUT when updating rows in this table.
        /// </summary>
        /// <remarks>
        /// This makes UpdateAndReturn methods atomic and more efficient.
        /// If the table has insert triggers, this must be false.
        /// https://stackoverflow.com/questions/13198476/cannot-use-update-with-output-clause-when-a-trigger-is-on-the-table
        /// </remarks>
        bool UpdateCanUseOutput { get; }

        /// <summary>
        /// Set this to true if the table has at least one update trigger.
        /// If this is true, then we can't use OUTPUT when updating a row
        /// https://stackoverflow.com/questions/13198476/cannot-use-update-with-output-clause-when-a-trigger-is-on-the-table
        /// </summary>
        [Obsolete("Use UpdateCanUseOutput. HasUpdateTrigger = !UpdateCanUseOutput. Obsolete since 2022-02-10.")]
        bool HasUpdateTrigger { get; }

        /// <summary>
        /// Will be used as "ORDER BY {OrderBy}" when selecting multiple rows from the table.
        /// </summary>
        string GetOrderBy(string columnPrefix = null);
    }
}