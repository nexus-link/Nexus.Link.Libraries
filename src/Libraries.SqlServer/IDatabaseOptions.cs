using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.SqlServer
{

    public delegate Task OnBeforeNewSqlConnectionAsync(string connectionString, CancellationToken cancellationToken = default);

    public interface IDatabaseOptions
    {
        string ConnectionString { get; }
        bool VerboseLogging { get; }
        OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; }
    }

    public class DatabaseOptions : IDatabaseOptions
    {
        /// <inheritdoc />
        public string ConnectionString { get; set; }

        /// <inheritdoc />
        public bool VerboseLogging { get; set; }

        /// <inheritdoc />
        public OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; set; }
    }
}