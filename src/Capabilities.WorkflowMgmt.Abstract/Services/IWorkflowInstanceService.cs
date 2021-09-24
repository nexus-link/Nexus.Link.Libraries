using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IWorkflowInstanceService : ICreateChildWithSpecifiedId<WorkflowInstanceCreate,WorkflowInstance, string>, IRead<WorkflowInstance, string>
    {
    }
}