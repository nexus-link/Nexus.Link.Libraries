using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract
{

    public interface IWorkflowCapability
    {
        IAsyncContextService AsyncContext { get; }
        IWorkflowFormService WorkflowForm { get; }
        IWorkflowVersionService WorkflowVersion{ get; }
        IWorkflowParameterService WorkflowParameter { get; }
        IActivityFormService ActivityForm { get; }
        IActivityVersionService ActivityVersion { get; }
        IActivityInstanceService ActivityInstance{ get; }
        ITransitionService Transition { get; }
        IActivityParameterService ActivityParameter { get; }
        IWorkflowInstanceService WorkflowInstance { get; }
        IWorkflowService Workflow { get; }
    }
}