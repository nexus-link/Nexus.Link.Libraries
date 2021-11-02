using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IWorkflowVersionService : 
        ICreateWithSpecifiedIdAndReturn<WorkflowVersionCreate, WorkflowVersion, string>, 
        IRead<WorkflowVersion, string>,
        IUpdateAndReturn<WorkflowVersion, string>
    {
    }
}