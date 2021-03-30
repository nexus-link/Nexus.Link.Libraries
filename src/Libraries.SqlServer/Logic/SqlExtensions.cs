using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.SqlServer.Logic
{

    public static class SqlExtensions
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private static readonly ICoolDownStrategy CoolDownStrategy = new CoolDownStrategy(TimeSpan.FromMinutes(1));

        public static async Task VerifyAvailabilityAsync(this IDbConnection connection, TimeSpan connectTimeout, ICoolDownStrategy coolDownStrategy = null, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open) return;
            coolDownStrategy = coolDownStrategy ?? CoolDownStrategy;
            var circuitBreaker = await _cache.GetOrCreateAsync(connection.ConnectionString, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                return Task.FromResult(new CircuitBreaker(coolDownStrategy));
            });

            await circuitBreaker.ExecuteOrThrowAsync(() => ConnectAsync(connection, connectTimeout, cancellationToken));
        }

        private static async Task ConnectAsync(IDbConnection connection, TimeSpan connectTimeout, CancellationToken cancellationToken)
        {
            var originalConnectionString = connection.ConnectionString;
            try
            {
                var connStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString)
                {
                    ConnectTimeout = (int)connectTimeout.TotalSeconds
                };
                connection.ConnectionString = connStringBuilder.ConnectionString;
                if (connection is SqlConnection sqlConnection) await sqlConnection.OpenAsync(cancellationToken);
                else connection.Open();
            }
            catch (Exception e)
            {
                var sqlConnection = connection as SqlConnection;
                var dataSource = sqlConnection?.DataSource ?? "(unknown data source)";
                throw new FulcrumResourceException($"Could not open database connection to {connection.Database} on {dataSource}", e);
            }
            finally
            {
                connection.ConnectionString = originalConnectionString;
            }
        }

        public static void ResetCache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}
