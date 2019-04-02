using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Libraries.Core.Logging
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    /// <summary>
    /// Represents a log message with properties such as correlation id, calling client, severity and the text message.
    /// </summary>
    public class LogRecord : IValidatable, ILoggable
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// The time that the log message was created
        /// Mandatory, i.e. must not be the default value.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// The context when this log record was created.
        /// </summary>
        [Obsolete("Use FulcrumApplication.Context.", true)]
        internal IDictionary<string, object> SavedContext { get; set; }

        /// <summary>
        /// The <see cref="LogSeverityLevel"/> of the log message
        /// </summary>
        public LogSeverityLevel SeverityLevel { get; set; }

        /// <summary>
        /// The logged message in plain text
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public JObject Data { get; internal set; }

        /// <summary>
        /// Information about an exception behind the message.
        /// Optional.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Where the log was issued (typically file name and line number)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The call stack for the moment when the logging was turned into it's own thread.
        /// </summary>
        public string StackTrace { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(TimeStamp, nameof(TimeStamp), errorLocation);
            //FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, TimeStamp, nameof(TimeStamp), errorLocation);
            FulcrumValidate.IsNotDefaultValue(SeverityLevel, nameof(SeverityLevel), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Message, nameof(Message), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString() => $"{SeverityLevel}: {Message}";

        /// <inheritdoc />
        public string ToLogString()
        {
            return ToLogString(true);
        }

        /// <summary>
        /// Summarize the information suitable for logging purposes.
        /// </summary>
        /// <param name="hideStackTrace">When this is true, any stack trace will be hidden.</param>
        public string ToLogString(bool hideStackTrace)
        {
            var correlationId = FulcrumApplication.Context.CorrelationId;
            var correlation = string.IsNullOrWhiteSpace(correlationId)
                ? ""
                : $" {correlationId}";
            var detailsLine =
                $"{TimeStamp.ToLogString()}{correlation} {SeverityLevel}";
            var context = ContextToLogString();
            if (!string.IsNullOrWhiteSpace(context)) detailsLine += $" {context}";
            var exceptionLine = "";
            var stackTraceLine = "";
            if (Exception != null) exceptionLine = $"\r{Exception.ToLogString(hideStackTrace)}";
            // TODO: Lars&Erik: Ska vi verkligen logga en StackTrace för själva loggen?
            if (!hideStackTrace && StackTrace != null && (Exception != null || IsGreaterThanOrEqualTo(LogSeverityLevel.Error)))
            {
                stackTraceLine = $"\r{StackTrace}";
            }
            return $"{detailsLine}\r{Message}{exceptionLine}{stackTraceLine}";
        }

        private string ContextToLogString()
        {
            var contextInfo =
                $"{FulcrumApplication.Setup.Tenant} {FulcrumApplication.Setup.Name} ({FulcrumApplication.Setup.RunTimeLevel}) context:{ FulcrumApplication.Context.ContextId}";
            var clientName = FulcrumApplication.Context.CallingClientName;

            var clientTenant = FulcrumApplication.Context.ClientTenant;
            if (clientTenant != null)
            {
                contextInfo += $" tenant: {clientTenant.ToLogString()}";
            }

            if (!string.IsNullOrWhiteSpace(clientName))
            {
                contextInfo += $" client: {clientName}";
            }

            return contextInfo;
        }

        /// <summary>
        /// Compares the current <see cref="SeverityLevel"/> with the supplied <paramref name="severityLevel"/>.
        /// </summary>
        /// <returns>True if the current level is greater than or equal to the value in the parameter <paramref name="severityLevel"/>.</returns>
        public bool IsGreaterThanOrEqualTo(LogSeverityLevel severityLevel)
        {
            return (int)SeverityLevel >= (int)severityLevel;
        }

        /// <inheritdoc />
#pragma warning disable 659
        public override bool Equals(object obj)
#pragma warning restore 659
        {
            if (!(obj is LogRecord logRecord)) return false;
            return Equals(logRecord.TimeStamp, TimeStamp)
                   && Equals(logRecord.Location, Location)
                   && Equals(logRecord.SeverityLevel, SeverityLevel)
                   && Equals(logRecord.Message, Message)
                   && Equals(logRecord.Exception, Exception)
                   && Equals(logRecord.StackTrace, StackTrace);
        }
    }
}
