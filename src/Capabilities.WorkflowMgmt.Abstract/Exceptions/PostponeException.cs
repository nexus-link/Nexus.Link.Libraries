using System;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions
{
    public class PostponeException : Exception
    {
        public Guid? RequestId { get; }

        public PostponeException(Guid requestId)
        {
            RequestId = requestId;
        }

        public PostponeException()
        {
        }
    }
}