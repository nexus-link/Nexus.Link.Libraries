using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;
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
        /// The optimization to use for GUIDs
        /// </summary>
        GuidOptimization? GuidOptimization { get; set; }
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

        public GuidOptimization? GuidOptimization { get; set; }
    }
}