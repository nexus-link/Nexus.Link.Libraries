using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State
{
    public interface IActivityInstanceService : ICreateWithSpecifiedIdAndReturn<ActivityInstanceCreate,ActivityInstance, string>, IRead<ActivityInstance, string>, IUpdateAndReturn<ActivityInstance, string>
    {

        Task SuccessAsync(string id, ActivityInstanceSuccessResult result, CancellationToken cancellationToken = default);
        Task FailedAsync(string id, ActivityInstanceFailedResult result, CancellationToken cancellationToken = default);
    }
}