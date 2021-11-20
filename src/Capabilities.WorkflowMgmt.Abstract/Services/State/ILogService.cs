using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Log = Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State.Log;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State
{
    public interface ILogService: ICreate<LogCreate, Log, string>
    {
        Task<PageEnvelope<Log>> ReadWorkflowChildrenWithPagingAsync(string workflowInstanceId,
            bool alsoActivityChildren, int offset, int? limit = null,
            CancellationToken cancellationToken = default);

        Task<PageEnvelope<Log>> ReadActivityChildrenWithPagingAsync(string workflowInstanceId, string activityFormId, int offset,
            int? limit = null,
            CancellationToken cancellationToken = default);
        
        Task DeleteWorkflowChildrenAsync(string workflowInstanceId, LogSeverityLevel? threshold = null, CancellationToken cancellationToken = default);
        Task DeleteActivityChildrenAsync(string workflowInstanceId, string activityFormId, LogSeverityLevel? threshold = null, CancellationToken cancellationToken = default);
    }
}