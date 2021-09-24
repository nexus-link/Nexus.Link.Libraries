using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IWorkflowFormService : ICreateWithSpecifiedId<WorkflowFormCreate,WorkflowForm, string>, IRead<WorkflowForm, string>, IUpdate<WorkflowForm, string>
    {
    }
}