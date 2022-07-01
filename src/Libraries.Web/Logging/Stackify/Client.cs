using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Logging.Stackify.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Web.Logging.Stackify
{
    /// <summary>
    /// Based on https://github.com/stackify/stackify-api/blob/master/endpoints/POST_Log_Save.md
    /// </summary>
    internal class Client
    {
        private readonly RestClient _client;
        private readonly string _serverKey;

        /// <inheritdoc />
        public Client(string serverKey)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serverKey, nameof(serverKey));
            _serverKey = serverKey;
            _client = new RestClient(new HttpSender(@"https://api.stackify.com/Log/Save"));
        }

        public async Task LogOneMessageAsync(Envelope envelope, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, List<string>>
            {
                {"Accept", new List<string>{"application/json"} },
                {"X-Stackify-PV", new List<string>{"V1"} },
                {"X-Stackify-Key", new List<string>{_serverKey} },
            };
            var response = await _client.PostAsync<JObject, Envelope>("", envelope, headers, cancellationToken);
        }
    }
}
