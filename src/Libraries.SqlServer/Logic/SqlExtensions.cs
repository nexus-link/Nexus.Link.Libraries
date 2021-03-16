using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    public static class SqlExtensions
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task VerifyAvailability(this IDbConnection connection, TimeSpan connectTimeout, TimeSpan considerConnectionBrokenFor, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open) return;

            var originalConnectionString = connection.ConnectionString;
            var cacheKey = $"{nameof(SqlExtensions)}/{originalConnectionString}";
            _cache.TryGetValue(cacheKey, out DateTimeOffset? timestamp);

            var sqlConnection = connection as SqlConnection;

            try
            {
                if (timestamp != null) throw new ApplicationException($"This connection is considered bad since {timestamp:O}");

                var connStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString)
                {
                    ConnectTimeout = (int) connectTimeout.TotalSeconds
                };
                try
                {
                    connection.ConnectionString = connStringBuilder.ConnectionString;
                    if (sqlConnection != null) await sqlConnection.OpenAsync(cancellationToken);
                    else connection.Open();
                }
                finally
                {
                    connection.ConnectionString = originalConnectionString;
                }
            }
            catch (Exception e)
            {
                if (timestamp == null)
                {
                    _cache.Set(cacheKey, DateTimeOffset.Now, DateTimeOffset.Now.Add(considerConnectionBrokenFor));
                }

                var dataSource = sqlConnection?.DataSource ?? "(unknown data source)";
                throw new FulcrumResourceException($"Could not open database connection to {connection.Database} on {dataSource}", e);
            }
        }

        public static void ResetCache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}
