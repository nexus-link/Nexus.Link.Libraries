using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State
{
    public interface IWorkflowSummaryService
    {
        Task<WorkflowSummary> GetSummaryAsync(string instanceId, CancellationToken cancellationToken = default);
        Task<WorkflowSummary> GetSummaryAsync(string formId, int majorVersion, string instanceId,
            CancellationToken cancellationToken = default);
    }
}