using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Misc.Models;
using Nexus.Link.Libraries.Core.Threads;

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

        /// <summary>
        /// Verify that the database connection is available by trying to open it.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="connectTimeout">The time that we are willing to wait for the DB response.
        /// NULL means that we should use the value from the connection string.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns></returns>
        public static async Task VerifyAvailabilityAsync(this IDbConnection connection, TimeSpan? connectTimeout = null, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open) return;

            await CircuitBreakerCollection.ExecuteOrThrowAsync(
                connection.ConnectionString,
                (t) => ConnectThreadSafeAsync(connection, connectTimeout, t),
                cancellationToken);
        }

        private static async Task ConnectThreadSafeAsync(IDbConnection connection, TimeSpan? connectTimeout, CancellationToken cancellationToken)
        {
            if (connection.State == ConnectionState.Open) return;
            var semaphore = new NexusAsyncSemaphore();
            await semaphore.ExecuteAsync(ct => ConnectAsync(connection, connectTimeout, ct), cancellationToken);
        }

        private static async Task ConnectAsync(IDbConnection connection, TimeSpan? connectTimeout, CancellationToken cancellationToken)
        {
            if (connection.State == ConnectionState.Open) return;
            var originalConnectionString = connection.ConnectionString;
            try
            {
                if (connectTimeout.HasValue)
                {
                    var connStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString)
                    {
                        ConnectTimeout = (int)connectTimeout.Value.TotalSeconds
                    };
                    connection.ConnectionString = connStringBuilder.ConnectionString;
                }
                
                if (connection is SqlConnection sqlConnection)
                {
                    await sqlConnection.OpenAsync(cancellationToken);
                    var a = sqlConnection.ClientConnectionId;
                }
                else
                {
                    connection.Open();
                }
            }
            catch (Exception e)
            {
                var sqlConnection = connection as SqlConnection;
                var database = connection.Database ?? "(unknown database)";
                var dataSource = sqlConnection?.DataSource ?? "(unknown data source)";
                var exception =
                    new FulcrumResourceException(
                        $"Could not open database connection to {database} on {dataSource}", e);
                throw new CircuitBreakerException(exception);
            }
            finally
            {
                if (connection.State != ConnectionState.Open)
                    connection.ConnectionString = originalConnectionString;
            }
        }

        /// <summary>
        /// Verify that the database connection is available by trying to open it.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="connectTimeout">The time that we are willing to wait for the DB response.
        /// NULL means that we should use the value from the connection string.</param>
        /// <returns></returns>
        public static void VerifyAvailability(this IDbConnection connection, TimeSpan? connectTimeout = null)
        {
            if (connection.State == ConnectionState.Open) return;

            CircuitBreakerCollection.ExecuteOrThrow(
                connection.ConnectionString,
                () => Connect(connection, connectTimeout));
        }

        private static void Connect(IDbConnection connection, TimeSpan? connectTimeout)
        {
            var originalConnectionString = connection.ConnectionString;
            try
            {
                if (connectTimeout.HasValue)
                {
                    var connStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString)
                    {
                        ConnectTimeout = (int)connectTimeout.Value.TotalSeconds
                    };
                    connection.ConnectionString = connStringBuilder.ConnectionString;
                }

                connection.Open();
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
                if (connection.State != ConnectionState.Open) connection.ConnectionString = originalConnectionString;
            }
        }
    }
}
