using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nexus.Link.Libraries.Core.Json
{
    public static class JsonExtensions
    {
        public static T AsCopy<T>(this T source) where T : class
        {
            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJsonString<T>(this T source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static T ToObjectFromJson<T>(this string source)
        {
            if (source == null) return default;
            return JsonConvert.DeserializeObject<T>(source);
        }

        /// <summary>
        /// Set the serializer settings the way we expect the to behave in Nexus Link
        /// </summary>
        /// <param name="settings"></param>
        public static JsonSerializerSettings SetAsNexusLink(this JsonSerializerSettings settings)
        {
            // We want indented formatting
            settings.Formatting = Formatting.Indented;

            //We want json serialization to ignore null values when writing json
            settings.NullValueHandling = NullValueHandling.Ignore;

            //We want the default behaviour when escaping strings
            settings.StringEscapeHandling = StringEscapeHandling.Default;

            // We want ISO date formats when serializing
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            // If no offset is mentioned, we will assume UTC, not local time.
            // https://stackoverflow.com/questions/58268931/json-deserialization-assumes-local-timezone-if-not-provided-for-datetimeoffset
            settings.DateParseHandling = DateParseHandling.None;
            settings.Converters.Add(new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });

            //We want enums to be serialized as strings and not numbers when writing json
            settings.Converters.Add(new StringEnumConverter());

            //We add a custom converter for handling StringValues
            settings.Converters.AddStringValuesConverter();

            return settings;
        }
    }
}
