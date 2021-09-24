using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IWorkflowParameterService : 
        ICreateDependentWithSpecifiedId<WorkflowParameterCreate,WorkflowParameter, string, string>, 
        IReadDependent<WorkflowParameter, string, string>,
        IReadChildrenWithPaging<WorkflowParameter, string>
    {
    }
}