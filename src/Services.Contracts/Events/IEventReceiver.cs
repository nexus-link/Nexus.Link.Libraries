using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Needs to be implemented by an adapter that subscribes to events
    /// </summary>
    public interface IEventReceiver : ICreate<JToken, string>
    {
    }
}