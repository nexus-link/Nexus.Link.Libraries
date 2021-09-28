using System;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions
{
    public class PostponeException : Exception
    {
        public string RequestId { get; }

        public PostponeException(string requestId)
        {
            RequestId = requestId;
        }

        public PostponeException()
        {
        }
    }
}