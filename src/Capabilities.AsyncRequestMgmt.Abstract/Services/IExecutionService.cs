using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services
{
    /// <summary>
    /// Operations for Executions.
    /// </summary>
    public interface IExecutionService
    {
        /// <summary>
        /// The caller is ready to try an execution again (was probably postponed before)
        /// </summary>
        Task ReadyForExecutionAsync(string executionId, CancellationToken cancellationToken = default);
    }
}