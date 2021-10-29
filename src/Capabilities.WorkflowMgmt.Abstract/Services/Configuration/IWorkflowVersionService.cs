using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IWorkflowVersionService : ICreateDependentWithSpecifiedId<WorkflowVersionCreate,WorkflowVersion, string, int>, IReadDependent<WorkflowVersion, string, int>, IUpdateDependent<WorkflowVersion, string, int>
    {
    }
}