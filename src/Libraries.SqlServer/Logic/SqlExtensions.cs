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
        public static ICircuitBreakerCollection CircuitBreakerCollection = new CircuitBreakerCollection(() =>
            new CircuitBreaker(new CircuitBreakerOptions
            {
                CoolDownStrategy = new CoolDownStrategy(TimeSpan.FromMinutes(1)),
                CancelConcurrentRequestsWhenOneFails = false
            }));
        public static async Task VerifyAvailabilityAsync(this IDbConnection connection, TimeSpan connectTimeout, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open) return;

            await CircuitBreakerCollection.ExecuteOrThrowAsync(
                connection.ConnectionString, 
                (t) => ConnectAsync(connection, connectTimeout, t), 
                cancellationToken);
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
                var database = connection.Database ?? "(unknown database)";
                var dataSource = sqlConnection?.DataSource ?? "(unknown data source)";
                var exception = new FulcrumResourceException($"Could not open database connection to {database} on {dataSource}", e);
                throw new CircuitBreakerException(exception);
            }
            finally
            {
                connection.ConnectionString = originalConnectionString;
            }
        }
    }
}
