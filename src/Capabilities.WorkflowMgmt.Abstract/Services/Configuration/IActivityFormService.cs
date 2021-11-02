using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IActivityFormService : ICreateWithSpecifiedIdAndReturn<ActivityFormCreate,ActivityForm, string>, IRead<ActivityForm, string>, IUpdateAndReturn<ActivityForm, string>
    {
    }
}