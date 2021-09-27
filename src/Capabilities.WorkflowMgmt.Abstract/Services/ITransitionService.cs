using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services
{
    public interface ITransitionService : ICreateChild<TransitionCreate,Transition, string>, IReadChildrenWithPaging<Transition, string>
    {
        Task<Transition> FindUniqueAsync(TransitionCreate transition, CancellationToken cancellationToken = default);
    }
}