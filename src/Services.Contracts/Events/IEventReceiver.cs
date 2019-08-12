using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Needs to be implemented by an adapter that subscribes to events
    /// </summary>
    public interface IEventReceiver
    {
        Task ReceiveEvent(JToken eventAsJson, CancellationToken token = default(CancellationToken));
    }
}