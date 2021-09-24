using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IActivityVersionService : ICreateChild<ActivityVersionCreate,ActivityVersion, string>, IUpdate<ActivityVersion, string>
    {
        public Task<ActivityVersion> FindUniqueByWorkflowVersionActivityAsync(string workflowVersionId, string activityId,
            CancellationToken cancellationToken = default);
    }
}