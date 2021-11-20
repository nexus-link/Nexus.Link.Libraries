﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration
{
    public interface ITransitionService : ICreateChild<TransitionCreate,Transition, string>, IReadChildrenWithPaging<Transition, string>
    {
        Task<Transition> FindUniqueAsync(string workflowVersionId, TransitionUnique transition, CancellationToken cancellationToken = default);
    }
}