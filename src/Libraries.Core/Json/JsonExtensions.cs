using Newtonsoft.Json;

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
    }
}
