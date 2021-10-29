using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract
{

    public interface IWorkflowMgmtCapability
    {
        IWorkflowFormService WorkflowForm { get; }
        IWorkflowVersionService WorkflowVersion{ get; }
        IWorkflowParameterService WorkflowParameter { get; }
        IActivityFormService ActivityForm { get; }
        IActivityVersionService ActivityVersion { get; }
        IActivityInstanceService ActivityInstance{ get; }
        ITransitionService Transition { get; }
        IActivityParameterService ActivityParameter { get; }
        IWorkflowInstanceService WorkflowInstance { get; }
        IWorkflowSummaryService WorkflowSummary { get; }
        IWorkflowService Workflow { get; }
    }
}