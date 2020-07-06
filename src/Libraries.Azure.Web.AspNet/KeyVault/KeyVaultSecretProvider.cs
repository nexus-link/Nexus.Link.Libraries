using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Azure.Web.AspNet.KeyVault
{
    public class KeyVaultSecretProvider : IKeyVaultSecretProvider
    {
        public async Task UpdateAppSettingsAsync(string vaultUrl, List<string> keyVaultSettings)
        {
            InternalContract.RequireNotNullOrWhiteSpace(vaultUrl, nameof(vaultUrl));
            InternalContract.RequireNotNull(keyVaultSettings, nameof(keyVaultSettings));

            var tokenProvider = new AzureServiceTokenProvider();

            var kvClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

            foreach (var setting in keyVaultSettings)
            {
                var vaultSecret = await kvClient.GetSecretAsync(vaultUrl, setting);
                ConfigurationManager.AppSettings.Set(setting, vaultSecret.Value);
            }
        }
    }
}