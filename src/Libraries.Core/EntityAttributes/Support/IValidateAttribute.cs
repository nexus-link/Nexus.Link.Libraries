using System;
using System.Reflection;

namespace Nexus.Link.Libraries.Core.EntityAttributes.Support
{
    public interface IValidateAttribute
    {
        ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo, object propertyValue, string errorLocation);
    }

    public class ValidationResult
    {
        public ValidationResult(string message = null)
        {
            if (message == null) return;
            IsValid = false;
            Message = message;
        }

        /// <summary>
        /// True if the property value is valid
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// If <see cref="IsValid"/> is false, this property contains an error messages, explaining why the property value is not valid.
        /// </summary>
        public string Message { get; set; }
    }
}