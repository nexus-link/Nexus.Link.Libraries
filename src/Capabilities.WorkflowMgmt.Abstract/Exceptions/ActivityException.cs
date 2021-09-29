using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions
{
    public class ActivityException : Exception
    {
        public string ExceptionType { get; }
        public string ExceptionMessage { get; }

        public ActivityException(string exceptionType, string exceptionMessage)
        {
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
        }
    }
}