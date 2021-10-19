using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IActivityVersionService : ICreate<ActivityVersionCreate, ActivityVersion, string>, IUpdate<ActivityVersion, string>
    {
        Task<ActivityVersion> FindUniqueAsync(string workflowVersionId, string activityFormId, CancellationToken cancellationToken = default);
    }
}