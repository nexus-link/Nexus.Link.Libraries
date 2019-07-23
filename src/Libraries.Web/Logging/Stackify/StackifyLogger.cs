using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Logging.Stackify.Model;

namespace Nexus.Link.Libraries.Web.Logging.Stackify
{
    /// <summary>
    /// Based on https://github.com/stackify/stackify-api/blob/master/endpoints/POST_Log_Save.md
    /// </summary>
    /// <inheritdoc cref="ISyncLogger" />
    public class StackifyLogger : ISyncLogger, IAsyncLogger
    {
        readonly ISyncLogger _queue;
        private readonly Client _client;
        private readonly string _env;
        private readonly string _serverName;
        private readonly string _appName;

        /// <inheritdoc />
        public StackifyLogger(string serverKey)
        {
            // Null serverKey is for testing, no log message is sent, but the LastSentEnvelope property is still set.
            if (!FulcrumApplication.IsInDevelopment)
            {
                InternalContract.RequireNotNullOrWhiteSpace(serverKey, nameof(serverKey));
            }
            _client = serverKey == null ? null : new Client(serverKey);
            _queue = new QueueToAsyncLogger(this);
            var tenant = FulcrumApplication.Setup.Tenant;
            _env = $"{tenant.Organization}/{tenant.Environment}";
            _serverName = FulcrumApplication.Setup.Name;
            _appName = $"{FulcrumApplication.Setup.Name} {(FulcrumApplication.Setup.RunTimeLevel)}";
        }

        /// <summary>
        /// The latest sent data. Intended for testing.
        /// </summary>
        public string LastSentEnvelope { get; private set; }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            _queue.LogSync(logRecord);
        }

        /// <inheritdoc />
        public async Task LogAsync(LogRecord logRecord)
        {
            if (logRecord == null) return;
            var envelope = new Envelope
            {
                Env = _env,
                ServerName = _serverName,
                AppName = _appName,
                Logger = $"Nexus {logRecord.SchemaName} ({logRecord.SchemaVersion})",
                Platform = "C#",
                Msgs = new List<Message>
                {
                    new Message
                    {
                        Msg = $"{logRecord.Message}\r{logRecord.Location}",
                        EpochMS = logRecord.TimeStamp.ToUnixTimeMilliseconds(),
                        Level = logRecord.SeverityLevel.ToString(),
                        SourceMethod = logRecord.Location,
                        TransId = FulcrumApplication.Context?.CorrelationId,
                        Data = logRecord.Data?.ToString(Formatting.Indented),
                        Ex = new Exception
                            {
                                Error = new ExceptionError
                                {
                                    Message = logRecord.Exception?.Message,
                                    ErrorType = logRecord.Exception?.GetType().FullName,
                                    CustomerName = FulcrumApplication.Context?.CallingClientName
                                }
                            }
                    }
                }

            };
            if (_client != null) await _client.LogOneMessageAsync(envelope);
            LastSentEnvelope = JsonConvert.SerializeObject(envelope);
        }
    }
}