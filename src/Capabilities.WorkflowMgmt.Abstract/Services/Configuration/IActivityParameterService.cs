using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IActivityParameterService : 
        ICreateDependentWithSpecifiedId<ActivityParameterCreate,ActivityParameter, string, string>, 
        IReadDependent<ActivityParameter, string, string>,
        IReadChildrenWithPaging<ActivityParameter, string>
    {
    }
}