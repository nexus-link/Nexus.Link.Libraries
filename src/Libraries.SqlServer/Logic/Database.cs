using System.Data.SqlClient;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    /// <summary>
    /// Base class for common Database knowledge
    /// </summary>
    public class Database
    {
        private readonly string _connectionString;

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
        /// Get a new SQL Connection
        /// </summary>
        /// <returns>A new <see cref="SqlConnection"/></returns>
        public virtual SqlConnection NewSqlConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
