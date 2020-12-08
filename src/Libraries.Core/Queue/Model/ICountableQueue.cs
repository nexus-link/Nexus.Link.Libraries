using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Queue.Model
{
    public interface ICountableQueue : IBaseQueue
    {
        Task<int?> GetApproximateMessageCountAsync();
    }
}