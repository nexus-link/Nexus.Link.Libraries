using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions
{
    public class SubRequestException : Exception
    {
        public SubRequestException(SubRequest subRequest) : base(subRequest.ToString())
        {
        }
    }
}