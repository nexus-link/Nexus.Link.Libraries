using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/* Unmerged change from project 'Libraries.Core (net461)'
Before:
using Nexus.Link.Libraries.Core.Assert;
After:
using Nexus;
using Nexus.Link;
using Nexus.Link.Libraries;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.EntityAttributes.Support;
*/
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.EntityAttributes
{
    public static class EntityAttributeExtensions
    {

        public static string GetPrimaryKeyPropertyName<T>(this T entity)
        {
            if (!typeof(T).IsClass) return null;
            var propertyInfo = Hint.GetPrimaryKeyProperty<T>();
            return propertyInfo?.Name;
        }

        public static PropertyInfo GetPropertyWithCustomAttribute<TCustomAttribute>(this object entity)
            where TCustomAttribute : Attribute
        {
            InternalContract.RequireNotNull(entity, nameof(entity));
            return entity.GetType().GetPropertiesWithCustomAttribute<TCustomAttribute>().FirstOrDefault();
        }

        public static void SetValueForPropertyWithCustomAttribute<TCustomAttribute>(this object entity, object value)
            where TCustomAttribute : Attribute
        {
            InternalContract.RequireNotNull(entity, nameof(entity));
            entity.TrySetValueForPropertyWithCustomAttribute<TCustomAttribute>(value);
        }

        public static bool TrySetValueForPropertyWithCustomAttribute<TCustomAttribute>(this object entity, object value)
            where TCustomAttribute : Attribute
        {
            InternalContract.RequireNotNull(entity, nameof(entity));
            var propertyInfo = entity.GetType().GetPropertiesWithCustomAttribute<TCustomAttribute>().FirstOrDefault();
            if (propertyInfo == null) return false;
            propertyInfo.SetValue(entity, value);
            return true;
        }

        public static bool TryGetValueForPropertyWithCustomAttribute<TCustomAttribute>(this object entity, out object value)
            where TCustomAttribute : Attribute
        {
            InternalContract.RequireNotNull(entity, nameof(entity));
            value = default;
            var propertyInfo = entity.GetType().GetPropertiesWithCustomAttribute<TCustomAttribute>().FirstOrDefault();
            if (propertyInfo == null) return false;
            value = propertyInfo.GetValue(entity);
            return true;
        }

        public static PropertyInfo GetPropertyWithCustomAttribute<TCustomAttribute>(this Type entityType)
            where TCustomAttribute : Attribute
        {
            InternalContract.RequireNotNull(entityType, nameof(entityType));
            return entityType.GetPropertiesWithCustomAttribute<TCustomAttribute>().FirstOrDefault();
        }
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute<TCustomAttribute>(this Type entityType)
            where TCustomAttribute : Attribute
        {
            return entityType.GetPropertiesWithCustomAttribute(typeof(TCustomAttribute));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute(this Type entityType, Type customAttributeType)
        {
            InternalContract.Require(typeof(Attribute).IsAssignableFrom(customAttributeType), $"Parameter {nameof(customAttributeType)} must be of type {nameof(Attribute)}");
            var properties = entityType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                var customAttribute = propertyInfo.GetCustomAttribute(customAttributeType);
                if (customAttribute == null) continue;
                yield return propertyInfo;
            }
        }
    }
}