using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IWorkflowFormService : ICreateWithSpecifiedId<WorkflowFormCreate,WorkflowForm, string>, IRead<WorkflowForm, string>, IUpdate<WorkflowForm, string>
    {
    }
}