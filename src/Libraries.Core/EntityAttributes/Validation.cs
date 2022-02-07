using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes.Support;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.EntityAttributes
{
    /// <summary>
    /// Attributes to validate entity properties and function parameters
    /// </summary>
    /// <remarks>
    /// Inspired by https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotation
    /// </remarks>
    public partial class Validation
    {
        public static IEnumerable<Type> AllPropertyValidations { get; } = new List<Type>()
        {
            typeof(NotNullAttribute),
            typeof(GreaterThanOrEqualToAttribute),
            typeof(GreaterThanOrEqualToPropertyAttribute),
            typeof(MatchRegularExpressionAttribute)
        };

        public class NotNullAttribute : PropertyValidationAttribute
        {
            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue)
            {
                return propertyValue != null
                    ? new ValidationResult() 
                    : new ValidationResult($"{propertyInfo.Name} must not be null");
            }
        }

        public class GreaterThanOrEqualToAttribute : PropertyValidationAttribute
        {
            private readonly object _expectedValue;

            public GreaterThanOrEqualToAttribute(object expectedValue)
            {
                _expectedValue = expectedValue;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue)
            {
                var propertyComparable = propertyValue as IComparable;
                var expectedComparable = _expectedValue as IComparable;
                if (propertyComparable == null || expectedComparable == null) return new ValidationResult();
                var compareValue = propertyComparable.CompareTo(expectedComparable);

                return compareValue >= 0 
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be >= {_expectedValue}.");
            }
        }

        public class GreaterThanOrEqualToPropertyAttribute : PropertyValidationAttribute
        {
            private readonly string _otherPropertyName;

            public GreaterThanOrEqualToPropertyAttribute(string otherPropertyName)
            {
                _otherPropertyName = otherPropertyName;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue)
            {
                InternalContract.RequireNotNull(entityType, nameof(entityType));
                InternalContract.RequireNotNull(entityValue, nameof(entityValue));
                var otherProperty = entityType.GetProperty(_otherPropertyName);
                if (otherProperty == null)
                {
                    // TODO: How do we handle this?
                    throw new FulcrumContractException($"Failed");
                }

                FulcrumAssert.IsNotNull(otherProperty, CodeLocation.AsString());
                var expectedValue = otherProperty.GetValue(entityValue);
                var propertyComparable = propertyValue as IComparable;
                var expectedComparable = expectedValue as IComparable;
                if (propertyComparable == null || expectedComparable == null) return new ValidationResult();
                var compareValue = propertyComparable.CompareTo(expectedComparable);

                return compareValue >= 0 
                    ? new ValidationResult() 
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be >= property {_otherPropertyName} ({expectedValue}).");
            }
        }

        public class MatchRegularExpressionAttribute : PropertyValidationAttribute
        {
            private readonly Regex _regularExpression;

            public MatchRegularExpressionAttribute(string regularExpression)
            {
                _regularExpression = new Regex(regularExpression);
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue)
            {
                if (!(propertyValue is string s)) s = propertyValue.ToString();

                return _regularExpression.IsMatch(s) 
                    ? new ValidationResult() 
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must match the regular expression \"{_regularExpression}\".");
            }
        }
        public static ValidationResult Validate(object entityValue, string errorLocation)
        {
            if (entityValue == null) return new ValidationResult();
            var entityType = entityValue.GetType();
            var properties = entityType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                foreach (var requireAttributeType in Validation.AllPropertyValidations)
                {
                    var customAttribute = propertyInfo.GetCustomAttribute(requireAttributeType);
                    if (customAttribute is IValidateAttribute requireAttribute)
                    {
                        var propertyValue = propertyInfo.GetValue(entityValue);
                        var validationResult = requireAttribute.Validate(entityType, entityValue, propertyInfo, propertyValue);
                        if (validationResult.IsValid) continue;
                        return validationResult;
                    }
                }
            }

            return new ValidationResult();
        }
    }
}