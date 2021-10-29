using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State
{
    public interface IWorkflowInstanceService : ICreateWithSpecifiedId<WorkflowInstanceCreate,WorkflowInstance, string>, IRead<WorkflowInstance, string>, IUpdate<WorkflowInstance, string>
    {
    }
}