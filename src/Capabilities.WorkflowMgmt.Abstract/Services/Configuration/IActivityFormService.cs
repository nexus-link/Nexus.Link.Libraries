using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IActivityFormService : ICreateWithSpecifiedId<ActivityFormCreate,ActivityForm, string>, IRead<ActivityForm, string>, IUpdate<ActivityForm, string>
    {
    }
}