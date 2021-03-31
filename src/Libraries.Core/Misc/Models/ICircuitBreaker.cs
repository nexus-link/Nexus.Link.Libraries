using System;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public interface ICircuitBreaker
    {
        int ConsecutiveErrors { get; }
        bool ExceptionDueToCircuitBreak { get; }
        DateTimeOffset? FirstFailureAt { get; }
        DateTimeOffset LastFailAt { get; }
        bool IsActive { get; }

        Task ExecuteOrThrowAsync(Func<Task> action);
    }
}