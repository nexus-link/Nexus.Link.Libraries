using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IWorkflowAdministrationService : IRead<Workflow, string>
    {
        /// <summary>
        /// Set a workflow in a cancelled state, which aborts the workflow process.
        /// </summary>
        Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
    }
}