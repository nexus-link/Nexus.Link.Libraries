using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Threads;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    /// <summary>
    /// Base class for common Database knowledge
    /// </summary>
    public class Database
    {
        private readonly string _connectionString;
        private readonly IDatabaseOptions _options;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="connectionString">How to connect to the database.</param>
        public Database(string connectionString)
        {
            InternalContract.RequireNotNullOrWhiteSpace(connectionString, nameof(connectionString));
            _connectionString = connectionString;
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="options">How to connect to the database.</param>
        public Database(IDatabaseOptions options) : this(options?.ConnectionString)
        {
            InternalContract.RequireNotNull(options, nameof(options));
            _options = options;
        }

        /// <summary>
        /// Get a new SQL Connection
        /// </summary>
        /// <remarks>
        ///If a <see cref="IDatabaseOptions"/> is provided, its <<see cref="IDatabaseOptions.OnBeforeNewSqlConnectionAsync"/> is invoked.
        /// </remarks>
        /// <returns>A new <see cref="SqlConnection"/></returns>
        [Obsolete("Use NewSqlConnectionAsync instead. Obsolete since 2021-10-21.", error: false)]
        public virtual SqlConnection NewSqlConnection()
        {
            if (_options?.OnBeforeNewSqlConnectionAsync != null)
            {
                ThreadHelper.CallAsyncFromSync(() => _options.OnBeforeNewSqlConnectionAsync(_connectionString));
            }
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Get a new SQL Connection
        /// </summary>
        /// <remarks>
        ///If a <see cref="IDatabaseOptions"/> is provided, its <<see cref="IDatabaseOptions.OnBeforeNewSqlConnectionAsync"/> is invoked.
        /// </remarks>
        /// <returns>A new <see cref="SqlConnection"/></returns>
        public virtual async Task<SqlConnection> NewSqlConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_options?.OnBeforeNewSqlConnectionAsync != null)
            {
                await _options.OnBeforeNewSqlConnectionAsync(_connectionString, cancellationToken);
            }
            return new SqlConnection(_connectionString);
        }
    }
}
