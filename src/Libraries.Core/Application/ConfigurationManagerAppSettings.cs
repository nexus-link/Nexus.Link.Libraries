using System.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Core.Application

{
    /// <summary>
    /// Implements <see cref="IAppSettingGetter"/> by using <see cref="ConfigurationManager"/>.
    /// </summary>
    public class ConfigurationManagerAppSettings : IAppSettingGetter
    {
        /// <inheritdoc />
        public string GetAppSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        /// <summary>
        /// Reads a json file, regarding it as a key value dictionary, and adds to the App Settings.
        /// </summary>
        /// <remarks>
        /// A suggestion is to have a git ignored local.settings.json (with Copy If Newer).
        /// </remarks>
        public ConfigurationManagerAppSettings WithLocalSettingsSupport(FileInfo jsonFile)
        {
            try
            {
                if (!jsonFile.Exists) return this;

                var localSettings = JObject.Parse(File.ReadAllText(jsonFile.FullName));
                foreach (var localSetting in localSettings)
                {
                    try
                    {
                        ConfigurationManager.AppSettings[localSetting.Key] = localSetting.Value.Value<string>();
                    }
                    catch
                    {
                        Log.LogError($"The setting '{localSetting.Key}' must be readable as a string in '{jsonFile.Name}', but was {localSetting.Value}");
                    }
                }
            }
            catch
            {
                // It's ok to not have a local file
            }

            return this;
        }
    }
}