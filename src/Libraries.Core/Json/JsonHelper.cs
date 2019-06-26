using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Model;

namespace Nexus.Link.Libraries.Core.Json
{
    /// <summary>
    /// Convenience methods for JSON stuff.
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// Try to deserialize a JSON string.
        /// </summary>
        /// <typeparam name="T">The type that the <paramref name="value"/> should be deserialized into.</typeparam>
        /// <param name="value">The JSON string that should be deserialized</param>
        /// <param name="deserializedObject">The resulting deserialized object or default(T) if the deserialization failed.</param>
        /// <returns>True if the deserialization was successful.</returns>
        public static bool TryDeserializeObject<T>(string value, out T deserializedObject)
        {
            deserializedObject = default(T);
            try
            {
                deserializedObject = JsonConvert.DeserializeObject<T>(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize a JSON string. If the deserialization fails we will return default(T).
        /// </summary>
        /// <typeparam name="T">The type that the <paramref name="value"/> should be deserialized into.</typeparam>
        /// <param name="value">The JSON string that should be deserialized</param>
        /// <returns>The deserialized object or default(T) if the serialization failed.</returns>
        public static T SafeDeserializeObject<T>(string value)
        {
            return !TryDeserializeObject(value, out T deserializedObject) ? default(T) : deserializedObject;
        }
    }
}
