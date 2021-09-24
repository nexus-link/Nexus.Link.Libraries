using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface IAsyncContextService
    {
        Task<AsyncExecutionContext> GetExecutionContextAsync(string executionId, RequestData requestData, CancellationToken cancellationToken = default);
        Task AddSubRequestAsync(string executionId, string identifier, SubRequest subRequest, CancellationToken cancellationToken = default);

        // TODO: Move to new service
        Task<string> GetStatusAsStringAsync(string requestId, CancellationToken cancellationToken = default);
        Task<JObject> GetStatusAsJsonAsync(string requestId, CancellationToken cancellationToken = default);

    }
}