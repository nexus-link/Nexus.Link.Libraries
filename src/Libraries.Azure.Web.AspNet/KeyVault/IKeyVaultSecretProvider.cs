using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Web.AspNet.KeyVault
{
    public interface IKeyVaultSecretProvider
    {
        Task UpdateAppSettingsAsync(string vaultUrl, List<string> keyVaultSettings);
    }
}