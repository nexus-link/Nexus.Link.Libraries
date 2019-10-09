using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Controllers.Events
{
    /// <summary>
    /// Service implementation of <see cref="IEventReceiver"/>
    /// </summary>
    [Route("api/NexusEventReceiver/v1/Events")]
    [ApiController]
    [Authorize(Policy = "HasMandatoryRole")]
    public class NexusReceiveEventsController: IEventReceiver
    {
        /// <summary>
        /// The logic for this controller
        /// </summary>
        protected readonly IEventReceiver Logic;

        /// <inheritdoc />
        public NexusReceiveEventsController(IEventReceiver logic)
        {
            Logic = logic;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task ReceiveEvent(JToken eventAsJson, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            var @event = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJson.ToString(Formatting.None));
            InternalContract.RequireNotNull(@event, nameof(@event));
            InternalContract.RequireNotNull(@event?.Metadata, nameof(@event.Metadata));
            InternalContract.RequireValidated(@event?.Metadata, nameof(@event.Metadata));
            await Logic.ReceiveEvent(eventAsJson, token);
        }
    }
}
