using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IActivityParameterService : 
        ICreateDependentWithSpecifiedId<ActivityParameterCreate,ActivityParameter, string, string>, 
        IReadDependent<ActivityParameter, string, string>,
        IReadChildrenWithPaging<ActivityParameter, string>
    {
    }
}