using System;

namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Properties required for record version.
    /// </summary>
    public interface IRecordVersion
    {
        /// <summary>
        /// This is intended for reading a column with the SQL Server rowversion.
        /// </summary>
        /// <remarks>Used in conjunction with <see cref="IOptimisticConcurrencyControlByETag"/></remarks>
        byte[] RecordVersion { get; set; }
    }
}
