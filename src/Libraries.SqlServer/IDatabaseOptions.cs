using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{

    public delegate Task OnBeforeNewSqlConnectionAsync(string connectionString, CancellationToken cancellationToken = default);

    public interface IDatabaseOptions
    {
        /// <summary>
        /// The connection string to connect to the database
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// True means that we should log all SQL statements
        /// </summary>
        bool VerboseLogging { get; }

        /// <summary>
        /// Set this if you have a distributed lock table that you want to use.
        /// </summary>
        IDistributedLockTable DistributedLockTable { get; set; }

        /// <summary>
        /// When a lock is claimed and the caller didn't specify a time span for the lock, then this time span will be used.
        /// </summary>
        TimeSpan DefaultLockTimeSpan { get; set; }

        /// <summary>
        /// These delegates will be called before we connect to a database. For instance to patch the database schema.
        /// </summary>
        OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; }

        /// <summary>
        /// This is the number that the database implementor has used as error number from triggers that are advanced constraints.
        /// </summary>
        int? TriggerConstraintSqlExceptionErrorNumber { get; set;  }

        /// <summary>
        /// This is the GUID generator that is used for row identifiers (this library doesn't use the GUID generator in SQL Server).
        /// If it is null, we will use <see cref="StorageHelper.GuidGenerator"/>.
        /// </summary>
        /// <remarks>
        /// Using the .NET built in GUID generator for identifiers can have a noticable negative effect for tables that have a lot of DELETE, e.g. a "queue" table.
        /// Indexes for such identifier colums (typically foreign keys to the table) will quickly become fragmented.
        /// Fragmentation of the table itself for non-sequential GUIDs can also be an issue, but there are other means to deal with that
        /// (using RecordCreatedAt as the clustered index), see https://docs.nexus.link/docs/sql-server-guidelines.
        /// </remarks>
        IGuidGenerator GuidGenerator { get; set; }
    }

    public class DatabaseOptions : IDatabaseOptions
    {
        /// <inheritdoc />
        public string ConnectionString { get; set; }

        /// <inheritdoc />
        public bool VerboseLogging { get; set; }
        
        /// <inheritdoc />
        public IDistributedLockTable DistributedLockTable { get; set; }

        /// <inheritdoc />
        public TimeSpan DefaultLockTimeSpan { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; set; }

        /// <inheritdoc />
        public int? TriggerConstraintSqlExceptionErrorNumber { get; set; }

        /// <inheritdoc />
        public IGuidGenerator GuidGenerator { get; set; }
    }
}