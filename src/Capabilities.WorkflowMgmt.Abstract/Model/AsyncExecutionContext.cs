using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model
{
    // TODO: Add Request (LatestRequest) and Response (LatestResponse)
    public class AsyncExecutionContext
    {
        public Guid ExecutionId { get; set; }

        public bool IsAsynchronous { get; set; }
        public Dictionary<string, SubRequest> SubRequests { get; } = new Dictionary<string, SubRequest>();

        public RequestData CurrentRequest { get; set; }
    }
}