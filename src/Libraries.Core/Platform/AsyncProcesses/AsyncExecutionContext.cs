using System;
using System.Collections.Generic;

namespace Nexus.Link.Libraries.Core.Platform.AsyncProcesses
{
    public class AsyncExecutionContext : ILockable
    {
        public Guid ExecutionId { get; set; }

        public int Version { get; set; }

        public bool IsAsynchronous { get; set; }
        public Dictionary<string, SubRequest> SubRequests { get; } = new Dictionary<string, SubRequest>();

        public string NextDescription { get; set; }
        public int NextSubRequestNumber { get; set; }

        // TODO: Hack to store a RequestData from Libraries.Web.AspNet
        public object CurrentRequest { get; set; }

        /// <inheritdoc />
        public LockWithTimeout LockWithTimeout { get; } = new LockWithTimeout();
    }
}