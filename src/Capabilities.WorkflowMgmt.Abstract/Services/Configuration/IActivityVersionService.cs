using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IActivityVersionService : ICreateWithSpecifiedIdAndReturn<ActivityVersionCreate, ActivityVersion, string>, IRead<ActivityVersion, string>, IUpdateAndReturn<ActivityVersion, string>
    {
    }
}