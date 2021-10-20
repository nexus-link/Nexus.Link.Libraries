using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services
{
    /// <summary>
    /// Operations for Execution responses.
    /// </summary>
    public interface IExecutionResponseService
    {
        /// <summary>
        /// Create an execution response
        /// </summary>
        /// <exception cref="FulcrumConflictException">If the execution had a response already</exception>
        Task CreateAsync(string executionId, ResponseData responseData, CancellationToken cancellationToken = default);
    }
}