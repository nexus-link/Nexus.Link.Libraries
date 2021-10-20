using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services
{
    /// <summary>
    /// Operations for Executions.
    /// </summary>
    public interface IExecutionService
    {
        /// <summary>
        /// Tell AM that we are ready to try an execution again (was probably postponed before)
        /// </summary>
        Task ReadyForExecutionAsync(string executionId, CancellationToken cancellationToken = default);
    }
}