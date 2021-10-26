using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IActivityInstanceService : ICreateAndReturn<ActivityInstanceCreate,ActivityInstance, string>, IRead<ActivityInstance, string>, IUpdateAndReturn<ActivityInstance, string>
    {
        Task<ActivityInstance> FindUniqueAsync(ActivityInstanceUnique findUnique, CancellationToken cancellationToken = default);
    }
}