using System;
using System.Collections.Generic;

namespace Nexus.Link.Libraries.Core.Platform.AsyncProcesses
{
    public class ExecutionContext : ILockable
    {
        public Guid ExecutionId { get; set; }

        public int Version { get; set; }

        public bool IsAsynchronous { get; set; }
        public Dictionary<string, SubRequest> SubRequests { get; } = new Dictionary<string, SubRequest>();

        public string NextDescription { get; set; }
        public int NextSubRequestNumber { get; set; }

        /// <inheritdoc />
        public LockWithTimeout LockWithTimeout { get; } = new LockWithTimeout();
    }
}