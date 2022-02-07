using System;
using System.Reflection;

namespace Nexus.Link.Libraries.Core.EntityAttributes.Support
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public abstract class PropertyValidationAttribute : Attribute, IValidateAttribute
    {
        public bool IsValid(Type entityType, object entityValue, PropertyInfo propertyInfo,
            object propertyValue)
        {
            var result = Validate(entityType, entityValue, propertyInfo, propertyValue);
            return result.IsValid;
        }

        public abstract ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
            object propertyValue);
    }
}