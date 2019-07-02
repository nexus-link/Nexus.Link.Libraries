using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;

namespace Nexus.Link.Libraries.Core.Decoupling
{
    public class SchemaParser
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Type>> _typeDictionary =
            new ConcurrentDictionary<string, ConcurrentDictionary<int, Type>>();

        /// <summary>
        /// Register a <paramref name="type"/> for an anonymous schema name and an unspecified schema version.
        /// </summary>
        /// <remarks>Same as registering the schema name "" and schema version 0.</remarks>
        public SchemaParser Add(Type type)
        {
            InternalContract.RequireNotNull(type, nameof(type));
            return Add("", 0, type);
        }

        /// <summary>
        /// Register a <paramref name="type"/> for an anonymous schema name and a specific <paramref name="schemaVersion"/>.
        /// </summary>
        /// <remarks>Same as registering the schema name "".</remarks>
        public SchemaParser Add(int schemaVersion, Type type)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, schemaVersion, nameof(schemaVersion));
            InternalContract.RequireNotNull(type, nameof(type));
            return Add("", schemaVersion, type);
        }

        /// <summary>
        /// Register a <paramref name="type"/> for a specific <paramref name="schemaName"/> and <paramref name="schemaVersion"/>.
        /// </summary>
        public SchemaParser Add(string schemaName, int schemaVersion, Type type)
        {
            InternalContract.RequireNotNull(schemaName, nameof(schemaName));
            InternalContract.RequireGreaterThanOrEqualTo(0, schemaVersion, nameof(schemaVersion));
            InternalContract.RequireNotNull(type, nameof(type));
            if (!_typeDictionary.TryGetValue(schemaName, out var versionDictionary))
            {
                versionDictionary = new ConcurrentDictionary<int, Type>();
                if (!_typeDictionary.TryAdd(schemaName, versionDictionary))
                {
                    versionDictionary = _typeDictionary[schemaName];
                }
            }

            var success = versionDictionary.TryAdd(schemaVersion, type);
            InternalContract.Require(success, $"Schema {schemaName} version {schemaVersion} had already been added.");

            return this;
        }

        /// <summary>
        /// Get the registered type for a specific <paramref name="schemaName"/> and <paramref name="schemaVersion"/>.
        /// </summary>
        private Type Get(string schemaName, int schemaVersion)
        {
            InternalContract.RequireNotNull(schemaName, nameof(schemaName));
            InternalContract.RequireGreaterThanOrEqualTo(0, schemaVersion, nameof(schemaVersion));
            if (!_typeDictionary.TryGetValue(schemaName, out var versionDictionary)) return null;
            return versionDictionary.TryGetValue(schemaVersion, out var type) ? type : null;
        }

        /// <summary>
        /// Try to convert a JSON string into an object of one of the registered types
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <param name="obj">The converted object.</param>
        /// <returns>True if the conversion was successful.</returns>
        public bool TryParse(string json, out object obj)
        {
            var isSuccess = TryParse(json, out _, out _, out var o);
            obj = o;
            return isSuccess;
        }

        /// <summary>
        /// Try to convert a JSON string into an object of one of the registered types
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <param name="schemaName">The schema name that was used for the conversion.</param>
        /// <param name="schemaVersion">The schema version that was used for the conversion.</param>
        /// <param name="obj">The converted object.</param>
        /// <returns>True if the conversion was successful.</returns>
        public bool TryParse(string json, out string schemaName, out int schemaVersion, out object obj)
        {
            schemaName = null;
            schemaVersion = 0;
            obj = null;
            if (json == null) return false;
            var probe = JsonHelper.SafeDeserializeObject<NamedSchema>(json);
            if (probe == null) return false;
            schemaName = probe.SchemaName ?? "";
            schemaVersion = probe.SchemaVersion;
            var type = Get(schemaName, schemaVersion);
            if (type == null) return false;
            obj = JsonConvert.DeserializeObject(json, type);
            return true;
        }
    }
}
