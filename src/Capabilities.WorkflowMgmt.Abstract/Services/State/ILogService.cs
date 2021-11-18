using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State
{
    public interface ILogService: ICreate<LogCreate, Log, string>
    {
        Task<PageEnvelope<Log>> ReadWorkflowChildrenWithPagingAsync(string workflowInstanceId,
            bool alsoActivityChildren, int offset, int? limit = null,
            CancellationToken cancellationToken = default);

        Task<PageEnvelope<Log>> ReadActivityChildrenWithPagingAsync(string activityInstanceId, int offset,
            int? limit = null,
            CancellationToken cancellationToken = default);
    }
}