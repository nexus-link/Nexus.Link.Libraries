using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Core.Translation
{
    public static class StringFinderExtension
    {
        public static string JoinStrings(this object o)
        {
            try
            {
                var objectType = o?.GetType();
                switch (o)
                {
                    case null:
                        return "";
                    case string s:
                        return s;
                    case ICollection collection:
                    {
                        return collection
                            .Cast<object>()
                            .Aggregate("", (current, item) => current + "," + JoinStrings(item));
                    }

                    default:
                        if (!objectType.IsClass) return "";
                        var properties = objectType
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        if (properties.Length == 0) return "";
                        return properties
                            .Where(p => p.CanRead)
                            .Aggregate("",
                                (current, property) =>
                                    current + $" {property.Name}=" + JoinStrings(GetPropertyValue(o, property)) + ";");
                }
            }
            catch (Exception e)
            {
                var objectAsJson = JsonConvert.SerializeObject(o, Formatting.Indented);
                Log.LogCritical($"Could not decorate object.\rObject: {objectAsJson}\rException: {e.Message}", e);
                throw;
            }
        }

        private static object GetPropertyValue(object o, PropertyInfo property)
        {
            return property.GetValue(o);
        }
    }
}
