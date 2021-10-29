using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration
{
    public interface IWorkflowService : IRead<Workflow, string>
    {
        /// <summary>
        /// Set a workflow in a cancelled state, which aborts the workflow process.
        /// </summary>
        Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retry an activity that is in a halted state
        /// </summary>
        Task RetryActivityAsync(string activityInstanceId, CancellationToken cancellationToken = default);
    }
}