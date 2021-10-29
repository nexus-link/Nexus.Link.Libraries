using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface IActivityVersionService : ICreate<ActivityVersionCreate, ActivityVersion, string>, IUpdate<ActivityVersion, string>
    {
        Task<ActivityVersion> FindUniqueAsync(string workflowVersionId, string activityFormId, CancellationToken cancellationToken = default);
    }
}