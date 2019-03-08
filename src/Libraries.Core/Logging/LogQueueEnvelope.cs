using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Libraries.Core.Logging
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    /// <summary>
    /// Adds additional information to a log record
    /// </summary>
    public class LogQueueEnvelope : ILoggable
    {
        /// <summary>
        /// The log record
        /// </summary>
        public LogRecord LogRecord { get; set; }

        /// <summary>
        /// The context when this log record was created.
        /// </summary>
        internal IDictionary<string, object> SavedContext { get; set; }

        /// <inheritdoc />
        public override string ToString() => LogRecord.ToString();

        /// <inheritdoc />
        public string ToLogString() => LogRecord.ToLogString();
    }
}
