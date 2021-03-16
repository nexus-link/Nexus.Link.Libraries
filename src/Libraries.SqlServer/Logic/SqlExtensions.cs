using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    public class CircuitBreaker
    {
        private readonly TimeSpan _coolDownTimeSpan;

        public enum StateEnum
        {
            Failed,
            ContenderIsTrying,
            Ok
        }

        private StateEnum _state;
        private readonly object _lock = new object();
        public DateTimeOffset LastFailAt { get; private set; }
        public DateTimeOffset NextTryAt { get; private set; }

        private bool TimeHasComeToTry => DateTimeOffset.Now >= NextTryAt;

        public CircuitBreaker(TimeSpan coolDownTimeSpan)
        {
            _coolDownTimeSpan = coolDownTimeSpan;
            _state = StateEnum.Ok;
        }

        public bool QuickFail
        {
            get
            {
                lock (_lock)
                {
                    switch (_state)
                    {
                        case StateEnum.Failed:
                            if (!TimeHasComeToTry) return true;
                            // When the time has come to do another try, we will let the first contender through.
                            _state = StateEnum.ContenderIsTrying;
                            return false;
                        case StateEnum.ContenderIsTrying:
                            // We have a contender. Deny everyone else.
                            return true;
                        case StateEnum.Ok:
                            // Everything is OK, go ahead.
                            return false;
                        default:
                            FulcrumAssert.Fail($"Unknown {typeof(StateEnum).FullName}: {_state}.");
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public void ReportResult(bool success)
        {
            if (success && _state == StateEnum.Ok) return;

            lock (_lock)
            {
                if (success)
                {
                    _state = StateEnum.Ok;
                    return;
                }
                _state = StateEnum.Failed;
                LastFailAt = DateTimeOffset.Now;
                NextTryAt = LastFailAt + _coolDownTimeSpan;
            }
        }

        public void ReportSuccess() => ReportResult(true);

        public void ReportFailure() => ReportResult(false);
    }

    public static class SqlExtensions
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task VerifyAvailability(this IDbConnection connection, TimeSpan connectTimeout, TimeSpan considerConnectionBrokenFor, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open) return;
            var cacheKey = $"{nameof(SqlExtensions)}/{connection.ConnectionString}";
            _cache.TryGetValue(cacheKey, out CircuitBreaker circuitBreaker);
            circuitBreaker = circuitBreaker ?? new CircuitBreaker(considerConnectionBrokenFor);

            var sqlConnection = connection as SqlConnection;

            try
            {
                if (circuitBreaker.QuickFail) throw new ApplicationException($"This connection has been considered bad since {circuitBreaker.LastFailAt:O}");
                var connStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString)
                {
                    ConnectTimeout = (int)connectTimeout.TotalSeconds
                };
                var originalConnectionString = connection.ConnectionString;
                connection.ConnectionString = connStringBuilder.ConnectionString;
                try
                {
                    if (sqlConnection != null) await sqlConnection.OpenAsync(cancellationToken);
                    else connection.Open();
                    circuitBreaker.ReportSuccess();
                    _cache.Remove(cacheKey);
                }
                catch (Exception e)
                {
                    circuitBreaker.ReportFailure();
                    // We will keep the information long enough to be available for sure when it is time to make another try 
                    var keepForSeconds = (connectTimeout.TotalSeconds + considerConnectionBrokenFor.TotalSeconds) * 2;
                    _cache.Set(cacheKey, circuitBreaker, DateTimeOffset.Now.AddSeconds(keepForSeconds));
                    throw;
                }
                finally
                {
                    connection.ConnectionString = originalConnectionString;
                }
            }
            catch (Exception e)
            {
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
