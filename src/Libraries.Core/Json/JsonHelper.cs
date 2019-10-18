using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        /// <param name="jObject">The object that should be deserialized</param>
        /// <param name="deserializedObject">The resulting deserialized object or default(T) if the deserialization failed.</param>
        /// <returns>True if the deserialization was successful.</returns>
        public static bool TryDeserializeObject<T>(JObject jObject, out T deserializedObject)
        {
            if (jObject != null) return TryDeserializeObject(jObject.ToString(Formatting.None), out deserializedObject);
            deserializedObject = default(T);
            return false;
        }

        /// <summary>
        /// Try to deserialize a JSON string.
        /// </summary>
        /// <param name="jObject">The object that should be deserialized</param>
        /// <param name="type">The type that the <paramref name="jObject"/> should be deserialized into.</param>
        /// <param name="deserializedObject">The resulting deserialized object or default(T) if the deserialization failed.</param>
        /// <returns>True if the deserialization was successful.</returns>
        public static bool TryDeserializeObject(JObject jObject, Type type, out object deserializedObject)
        {
            if (jObject != null) return TryDeserializeObject(jObject.ToString(Formatting.None), out deserializedObject);
            deserializedObject = null;
            return false;
        }

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
        /// Try to deserialize a JSON string.
        /// </summary>
        /// <param name="value">The JSON string that should be deserialized</param>
        /// <param name="type">The type that the <paramref name="value"/> should be deserialized into.</param>
        /// <param name="deserializedObject">The resulting deserialized object or null if the deserialization failed.</param>
        /// <returns>True if the deserialization was successful.</returns>
        public static bool TryDeserializeObject(string value, Type type, out object deserializedObject)
        {
            deserializedObject = null;
            try
            {
                deserializedObject = JsonConvert.DeserializeObject(value, type);
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
        /// <typeparam name="T">The type that the <paramref name="jObject"/> should be deserialized into.</typeparam>
        /// <param name="jObject">The object that should be deserialized</param>
        /// <returns>The deserialized object or default(T) if the serialization failed.</returns>
        public static T SafeDeserializeObject<T>(JObject jObject)
        {
            return jObject == null ? 
                default(T) :
                SafeDeserializeObject<T>(jObject.ToString(Formatting.None));
        }

        /// <summary>
        /// Deserialize a JSON string. If the deserialization fails we will return default(T).
        /// </summary>
        /// <param name="jObject">The object that should be deserialized</param>
        /// <param name="type">The type that the <paramref name="jObject"/> should be deserialized into.</param>
        /// <returns>The deserialized object or default(T) if the serialization failed.</returns>
        public static object SafeDeserializeObject(JObject jObject, Type type)
        {
            return jObject == null ? 
                null :
                SafeDeserializeObject(jObject.ToString(Formatting.None), type);
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

        /// <summary>
        /// Deserialize a JSON string. If the deserialization fails we will return default(T).
        /// </summary>
        /// <param name="value">The JSON string that should be deserialized</param>
        /// <param name="type">The type that the <paramref name="value"/> should be deserialized into.</param>
        /// <returns>The deserialized object or default(T) if the serialization failed.</returns>
        public static object SafeDeserializeObject(string value, Type type)
        {
            return !TryDeserializeObject(value, type, out var deserializedObject) ? null : deserializedObject;
        }
    }
}
