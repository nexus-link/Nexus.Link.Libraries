using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Web.Clients
{
    public static class ClientConfigurationHelper
    {
        public static ClientConfiguration GetConfigurationForClient(ILeverConfiguration configuration, string client)
        {
            var clientConfigurations = GetClientsConfigurations(configuration);
            if (clientConfigurations == null) return null;

            if (!clientConfigurations.TryGetValue(client, out var clientConfiguration)) return null;

            // Check that we don't get multiple Authorization headers
            if (clientConfiguration.RequestHeaders == null) return clientConfiguration;
            if (!string.IsNullOrWhiteSpace(clientConfiguration.Authentication))
            {
                if (clientConfiguration.RequestHeaders.Any(x => x.Key.Equals("Authorization")))
                {
                    throw new FulcrumBusinessRuleException($"[{FulcrumApplication.Setup.Name}] Client configuration error ({client}). You cannot both have a refernece to an 'Authentication' configuration and an 'Authorization' custom header.");
                }
            }

            return clientConfiguration;
        }

        public static Dictionary<string, ClientConfiguration> GetClientsConfigurations(ILeverConfiguration configuration)
        {
            var tenantClientSettingJToken = configuration?.Value<JToken>("Clients");
            if (tenantClientSettingJToken == null) return null;
            var clientConfigurations = JsonConvert.DeserializeObject<List<ClientConfiguration>>(tenantClientSettingJToken.ToString());
            return clientConfigurations.ToDictionary(x => x.Name, x => x);
        }

    }
}
