using System;

namespace Nexus.Link.Libraries.Core.Platform.AsyncProcesses
{
    public class SubRequest
    {
        public string Identifier { get; set; }

        public SubRequest(string identifier)
        {
            Identifier = identifier;
        }

        public Guid? RequestId { get; set; }
        public bool HasCompleted { get; set; }
        public string ExceptionTypeName { get; set; }
        public string ResultValueAsJson { get; set; }
        public string Description { get; set; }

        public bool HasFailed => HasCompleted && ExceptionTypeName != null;

        public string StateAsString => HasCompleted ? HasFailed ? $"Failed ({ExceptionTypeName})" : "Completed" : "";

        public override string ToString() => $"{Identifier} ({Description}) {StateAsString}";
    }
}