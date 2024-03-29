﻿using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Nexus.Link.Libraries.Core.Application
{
    /// <summary>
    /// Methods for accessing the app settings
    /// </summary>
    public class AppSettings
    {
        private readonly IAppSettingGetter _appSettingGetter;

#if NETSTANDARD
        private readonly IConfiguration _configuration;
        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }
#endif

        /// <summary>
        /// Can get app settings by using the <paramref name="appSettingGetter"/>.
        /// </summary>
        /// <param name="appSettingGetter"></param>
        public AppSettings(IAppSettingGetter appSettingGetter)
        {
            _appSettingGetter = appSettingGetter;
        }
        /// <summary>
        /// Get a string value.
        /// </summary>
        /// <param name="name">The name of the app setting.</param>
        /// <param name="isMandatory">Throw an excepton if this is true and no value was found.</param>
        /// <returns>The found string or null.</returns>
        public string GetString(string name, bool isMandatory)
        {
            // We must not have InternalContract and stuff here, since we may not have set up logging, etc.
            if (string.IsNullOrWhiteSpace(name)) throw new FulcrumContractException($"Parameter {nameof(name)} was empty.");
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            var value = _appSettingGetter?.GetAppSetting(name)
#if NETSTANDARD
                        ?? _configuration[name]
#endif
            ;
            if (isMandatory && string.IsNullOrWhiteSpace(value)) throw new FulcrumContractException($"Missing app setting: {name}");
            return value;
        }

        /// <summary>
        /// Get an enumeration value.
        /// </summary>
        /// <param name="name">The name of the app setting.</param>
        /// <param name="isMandatory">Throw an excepton if this is true and no value was found.</param>
        /// <typeparam name="T">The enumeration type</typeparam>
        /// <returns>The found enumeration value or null.</returns>
        public T GetEnum<T>(string name, bool isMandatory) where T : struct
        {
            // We must not have InternalContract and stuff here, since we may not have set up logging, etc.
            if (string.IsNullOrWhiteSpace(name)) throw new FulcrumContractException($"Parameter {nameof(name)} was empty.");
            var valueAsString = GetString(name, isMandatory);
            if (valueAsString == null) return default;
            if (!Enum.TryParse(valueAsString, true, out T value)) throw new FulcrumContractException($"App setting {name} ({valueAsString}) must have one of the values for {typeof(T).FullName}.");
            return value;
        }

        /// <summary>
        /// Get an <see cref="Tenant"/>.
        /// </summary>
        /// <param name="organizationSettingName">The name of the app setting for the organization part of the tenant.</param>
        /// <param name="environmentSettingName">The name of the app setting for the environment part of the tenant.</param>
        /// <param name="isMandatory">Throw an excepton if this is true and no value was found.</param>
        /// <returns>The found enumeration value or null.</returns>
        public Tenant GetTenant(string organizationSettingName, string environmentSettingName, bool isMandatory)
        {
            var organization = GetString(organizationSettingName, isMandatory);
            if (organization == null) return null;
            var environment = GetString(environmentSettingName, isMandatory);
            return environment == null ? null : new Tenant(organization, environment);
        }
    }
}
