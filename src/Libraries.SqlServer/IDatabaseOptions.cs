using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.SqlServer
{

    public delegate Task OnBeforeNewSqlConnectionAsync(string connectionString, CancellationToken cancellationToken = default);

    public interface IDatabaseOptions
    {
        string ConnectionString { get; }
        OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; }
    }

    internal class DatabaseOptions : IDatabaseOptions
    {
        public DatabaseOptions(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
        public OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync => null;
    }
}