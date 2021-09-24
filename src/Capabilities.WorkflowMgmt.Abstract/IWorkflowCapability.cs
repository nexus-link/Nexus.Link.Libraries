using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract
{

    public interface IWorkflowCapability
    {
        public IAsyncContextService AsyncContext { get; }
        public IWorkflowFormService WorkflowForm { get; }
        public IWorkflowVersionService WorkflowVersion{ get; }
        public IWorkflowParameterService WorkflowParameter { get; }
        public IActivityFormService ActivityForm { get; }
        public IActivityVersionService ActivityVersion { get; }
        public IActivityInstanceService ActivityInstance{ get; }
        public ITransitionService Transition { get; }
        public IActivityParameterService ActivityParameter { get; }
        public IWorkflowInstanceService WorkflowInstance { get; }
    }
}