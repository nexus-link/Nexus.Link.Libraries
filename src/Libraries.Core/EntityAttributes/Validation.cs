using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
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
    public class Validation
    {
        /// <summary>
        /// The list of all property validation attributes
        /// </summary>
        public static IEnumerable<Type> AllPropertyValidationAttributes { get; } = new List<Type>()
        {
            typeof(NotNullAttribute),
            typeof(NotNullOrEmptyAttribute),
            typeof(NotNullOrWhitespaceAttribute),
            typeof(NotEqualToAttribute),
            typeof(NotEqualToPropertyAttribute),
            typeof(NotDefaultAttribute),
            typeof(GreaterThanAttribute),
            typeof(NotEqualToAttribute),
            typeof(GreaterThanOrEqualToAttribute),
            typeof(GreaterThanPropertyAttribute),
            typeof(GreaterThanOrEqualToPropertyAttribute),
            typeof(LessThanAttribute),
            typeof(LessThanOrEqualToAttribute),
            typeof(LessThanPropertyAttribute),
            typeof(LessThanOrEqualToPropertyAttribute),
            typeof(MatchRegularExpressionAttribute),
            typeof(NotMatchRegularExpressionAttribute),
            typeof(InEnumerationAttribute),
            typeof(JSonStringAttribute),
            typeof(LowerCaseAttribute),
            typeof(UpperCaseAttribute),
            typeof(ValidateAttribute)
        };

        /// <summary>
        /// The property value must not be equal to null
        /// </summary>
        public class NotNullAttribute : PropertyValidationAttribute
        {
            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                return propertyValue != null
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} must not be null");
            }
        }

        /// <summary>
        /// The property value must not be null and the value as a string must not be the empty string, e.g. ""
        /// </summary>
        public class NotNullOrEmptyAttribute : PropertyValidationAttribute
        {
            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                return propertyValue != null && propertyValue is string valueAsString && !string.IsNullOrEmpty(valueAsString)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} must not be null or empty.");
            }
        }

        /// <summary>
        /// The property value must not be null and the value as a string must not be the empty string, e.g. "", and not consist only of whitespace.
        /// </summary>
        public class NotNullOrWhitespaceAttribute : PropertyValidationAttribute
        {
            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                return propertyValue != null && propertyValue is string valueAsString && !string.IsNullOrWhiteSpace(valueAsString)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} must not be null, empty or only contain whitespace.");
            }
        }

        /// <summary>
        /// The property value must not be the default value. E.g. not 0 for <see cref="int"/> and not DateTimeOffset.MinValue for <see cref="DateTimeOffset"/>.
        /// </summary>
        public class NotDefaultAttribute : PropertyValidationAttribute
        {
            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                InternalContract.RequireNotNull(propertyInfo, nameof(propertyInfo));
                var defaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
                return propertyValue != null && !propertyValue.Equals(defaultValue)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} must not be the default value ({defaultValue}) or null.");
            }
        }

        /// <summary>
        /// The property value must not be equal to the given reference value.
        /// </summary>
        public class NotEqualToAttribute : PropertyValidationAttribute
        {
            public object ReferenceValue { get; protected set; }

            public NotEqualToAttribute(object referenceValue)
            {
                ReferenceValue = referenceValue;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                return !Equals(propertyValue, ReferenceValue)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be != {ReferenceValue}.");
            }
        }

        /// <summary>
        /// The property value must not be equal to another property in the same object, with the given name.
        /// </summary>
        public class NotEqualToPropertyAttribute : PropertyValidationAttribute
        {
            public string OtherPropertyName { get; protected set; }

            public NotEqualToPropertyAttribute(string otherPropertyName)
            {
                OtherPropertyName = otherPropertyName;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                InternalContract.RequireNotNull(entityType, nameof(entityType));
                InternalContract.RequireNotNull(entityValue, nameof(entityValue));
                var otherProperty = entityType.GetProperty(OtherPropertyName);
                if (otherProperty == null)
                {
                    // TODO: How do we handle this?
                    throw new FulcrumContractException($"Failed");
                }

                FulcrumAssert.IsNotNull(otherProperty, CodeLocation.AsString());
                var expectedValue = otherProperty.GetValue(entityValue);
                return !Equals(propertyValue, expectedValue)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be != property {OtherPropertyName} ({expectedValue}).");
            }
        }

        /// <summary>
        /// The property value must be greater than the given reference value.
        /// </summary>
        public class GreaterThanAttribute : PropertyValidationAttribute
        {
            public object ReferenceValue { get; protected set; }

            public GreaterThanAttribute(object referenceValue)
            {
                ReferenceValue = referenceValue;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                if (!TryCompare(ReferenceValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }
                return compareValue > 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be > {ReferenceValue}.");
            }
        }

        /// <summary>
        /// The property value must be greater than or equal to the given reference value.
        /// </summary>
        public class GreaterThanOrEqualToAttribute : PropertyValidationAttribute
        {
            public object ReferenceValue { get; protected set; }

            public GreaterThanOrEqualToAttribute(object referenceValue)
            {
                ReferenceValue = referenceValue;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                if (!TryCompare(ReferenceValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }

                return compareValue >= 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be >= {ReferenceValue}.");
            }
        }

        /// <summary>
        /// The property value must be greater than the specified property in the same object.
        /// </summary>
        public class GreaterThanPropertyAttribute : PropertyValidationAttribute
        {
            public string OtherPropertyName { get; protected set; }

            public GreaterThanPropertyAttribute(string otherPropertyName)
            {
                OtherPropertyName = otherPropertyName;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                InternalContract.RequireNotNull(entityType, nameof(entityType));
                InternalContract.RequireNotNull(entityValue, nameof(entityValue));
                var otherProperty = entityType.GetProperty(OtherPropertyName);
                if (otherProperty == null)
                {
                    // TODO: How do we handle this?
                    throw new FulcrumContractException($"Failed");
                }

                FulcrumAssert.IsNotNull(otherProperty, CodeLocation.AsString());
                var expectedValue = otherProperty.GetValue(entityValue);
                if (!TryCompare(expectedValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }

                return compareValue > 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be > property {OtherPropertyName} ({expectedValue}).");
            }
        }

        /// <summary>
        /// The property value must be greater than or equal to the specified property in the same object.
        /// </summary>
        public class GreaterThanOrEqualToPropertyAttribute : PropertyValidationAttribute
        {
            public string OtherPropertyName { get; protected set; }

            public GreaterThanOrEqualToPropertyAttribute(string otherPropertyName)
            {
                OtherPropertyName = otherPropertyName;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                InternalContract.RequireNotNull(entityType, nameof(entityType));
                InternalContract.RequireNotNull(entityValue, nameof(entityValue));
                var otherProperty = entityType.GetProperty(OtherPropertyName);
                if (otherProperty == null)
                {
                    // TODO: How do we handle this?
                    throw new FulcrumContractException($"Failed");
                }

                FulcrumAssert.IsNotNull(otherProperty, CodeLocation.AsString());
                var expectedValue = otherProperty.GetValue(entityValue);
                if (!TryCompare(expectedValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }

                return compareValue >= 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be >= property {OtherPropertyName} ({expectedValue}).");
            }
        }

        /// <summary>
        /// The property value must be less than the given reference value.
        /// </summary>
        public class LessThanAttribute : PropertyValidationAttribute
        {
            public object ReferenceValue { get; protected set; }

            public LessThanAttribute(object referenceValue)
            {
                ReferenceValue = referenceValue;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                if (!TryCompare(ReferenceValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }
                return compareValue < 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be < {ReferenceValue}.");
            }
        }

        /// <summary>
        /// The property value must be less than or equal to the given reference value.
        /// </summary>
        public class LessThanOrEqualToAttribute : PropertyValidationAttribute
        {
            public object ReferenceValue { get; protected set; }

            public LessThanOrEqualToAttribute(object referenceValue)
            {
                ReferenceValue = referenceValue;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                if (!TryCompare(ReferenceValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }

                return compareValue <= 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be <= {ReferenceValue}.");
            }
        }

        /// <summary>
        /// The property value must be less than the specified other property in the same object.
        /// </summary>
        public class LessThanPropertyAttribute : PropertyValidationAttribute
        {
            public string OtherPropertyName { get; protected set; }

            public LessThanPropertyAttribute(string otherPropertyName)
            {
                OtherPropertyName = otherPropertyName;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                InternalContract.RequireNotNull(entityType, nameof(entityType));
                InternalContract.RequireNotNull(entityValue, nameof(entityValue));
                var otherProperty = entityType.GetProperty(OtherPropertyName);
                if (otherProperty == null)
                {
                    // TODO: How do we handle this?
                    throw new FulcrumContractException($"Failed");
                }

                FulcrumAssert.IsNotNull(otherProperty, CodeLocation.AsString());
                var expectedValue = otherProperty.GetValue(entityValue);
                if (!TryCompare(expectedValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }

                return compareValue < 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be < property {OtherPropertyName} ({expectedValue}).");
            }
        }

        /// <summary>
        /// The property value must be less than or equal to the specified other property in the same object.
        /// </summary>
        public class LessThanOrEqualToPropertyAttribute : PropertyValidationAttribute
        {
            public string OtherPropertyName { get; protected set; }

            public LessThanOrEqualToPropertyAttribute(string otherPropertyName)
            {
                OtherPropertyName = otherPropertyName;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                InternalContract.RequireNotNull(entityType, nameof(entityType));
                InternalContract.RequireNotNull(entityValue, nameof(entityValue));
                var otherProperty = entityType.GetProperty(OtherPropertyName);
                if (otherProperty == null)
                {
                    // TODO: How do we handle this?
                    throw new FulcrumContractException($"Failed");
                }

                FulcrumAssert.IsNotNull(otherProperty, CodeLocation.AsString());
                var expectedValue = otherProperty.GetValue(entityValue);
                if (!TryCompare(expectedValue, propertyValue, out var compareValue))
                {
                    return new ValidationResult();
                }

                return compareValue <= 0
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be <= property {OtherPropertyName} ({expectedValue}).");
            }
        }

        /// <summary>
        /// The property value must match the specified regular expression
        /// </summary>
        public class MatchRegularExpressionAttribute : PropertyValidationAttribute
        {
            public Regex RegularExpression { get; protected set; }

            public MatchRegularExpressionAttribute(string regularExpression, RegexOptions options = 0)
            {
                RegularExpression = new Regex(regularExpression, options);
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();
                if (!(propertyValue is string s)) s = propertyValue.ToString();

                return RegularExpression.IsMatch(s)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must match the regular expression \"{RegularExpression}\".");
            }
        }

        /// <summary>
        /// The property value must not match the specified regular expression
        /// </summary>
        public class NotMatchRegularExpressionAttribute : PropertyValidationAttribute
        {
            public Regex RegularExpression { get; protected set; }

            public NotMatchRegularExpressionAttribute(string regularExpression)
            {
                RegularExpression = new Regex(regularExpression);
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();
                if (!(propertyValue is string s)) s = propertyValue.ToString();

                return !RegularExpression.IsMatch(s)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must match the regular expression \"{RegularExpression}\".");
            }
        }

        /// <summary>
        /// The property value must be part of the given enumeration type.
        /// </summary>
        public class InEnumerationAttribute : PropertyValidationAttribute
        {
            public Type EnumerationType { get; protected set; }

            public InEnumerationAttribute(Type enumerationType)
            {
                InternalContract.RequireNotNull(enumerationType, nameof(enumerationType));
                InternalContract.Require(enumerationType.IsEnum, $"Parameter {nameof(enumerationType)} must be of type enum.");
                EnumerationType = enumerationType;
            }

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();

                return Enum.IsDefined(EnumerationType, propertyValue)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must represent one of the enumeration values for ({EnumerationType.FullName}).");
            }
        }

        /// <summary>
        /// The property value must be a JSON string
        /// </summary>
        public class JSonStringAttribute : PropertyValidationAttribute
        {
            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();
                try
                {
                    JToken.Parse(propertyValue as string);
                    return new ValidationResult();
                }
                catch (Exception e)
                {
                    return new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be a string that is a JSON expression: {e.Message}");
                }
            }
        }

        /// <summary>
        /// The property value as a string must be in lower case
        /// </summary>
        public class LowerCaseAttribute : PropertyValidationAttribute
        {
            public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();
                var valueAsString = propertyValue.ToString();
                return valueAsString == valueAsString.ToLower(CultureInfo)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be in lower case.");
            }
        }

        /// <summary>
        /// The property value as a string must be in upper case
        /// </summary>
        public class UpperCaseAttribute : PropertyValidationAttribute
        {
            public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();
                var valueAsString = propertyValue.ToString();
                return valueAsString == valueAsString.ToUpper(CultureInfo)
                    ? new ValidationResult()
                    : new ValidationResult($"{propertyInfo.Name} ({propertyValue}) must be in upper case.");
            }
        }

        /// <summary>
        /// The property value is expected to be an object or an enumeration of objects that should be validated
        /// </summary>
        public class ValidateAttribute : PropertyValidationAttribute
        {
            public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

            public override ValidationResult Validate(Type entityType, object entityValue, PropertyInfo propertyInfo,
                object propertyValue, string errorLocation)
            {
                if (propertyValue == null) return new ValidationResult();
                if (propertyValue is IEnumerable<object> enumerable)
                {
                    foreach (var o in enumerable)
                    {
                        var result = Validation.Validate(o, errorLocation);
                        if (!result.IsValid) return result;
                    }
                    return new ValidationResult();
                }
                return Validation.Validate(propertyValue, errorLocation);
            }
        }

        public static ValidationResult Validate(object entityValue, string errorLocation)
        {
            if (entityValue == null) return new ValidationResult();
            var entityType = entityValue.GetType();
            var properties = entityType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                var customAttributes = propertyInfo.GetCustomAttributes();
                foreach (var customAttribute in customAttributes)
                {
                    if (!(customAttribute is IValidateAttribute requireAttribute)) continue;
                    if (TriggerSaysSkipValidation(requireAttribute)) continue;
                    var propertyValue = propertyInfo.GetValue(entityValue);
                    var validationResult = requireAttribute.Validate(entityType, entityValue, propertyInfo, propertyValue, errorLocation);
                    if (validationResult.IsValid) continue;
                    return validationResult;
                }
            }

            return new ValidationResult();

            bool TriggerSaysSkipValidation(IValidateAttribute requireAttribute)
            {
                if (!(requireAttribute is PropertyValidationAttribute propertyValidationAttribute))
                {
                    return false;
                }
                var triggerPropertyName = propertyValidationAttribute.TriggerPropertyName;
                if (triggerPropertyName == null)
                {
                    return false;
                }
                var otherProperty = entityType.GetProperty(triggerPropertyName);
                if (otherProperty == null)
                {
                    InternalContract.Fail($"Expected entity type {entityType.Name} to have a property named {triggerPropertyName}.");
                    return false;
                }

                if (!(otherProperty.GetValue(entityValue) is bool triggerValue))
                {
                    return false;
                }
                return triggerValue && propertyValidationAttribute.InvertedTrigger 
                       || !triggerValue && !propertyValidationAttribute.InvertedTrigger;
            }
        }

        private static bool TryCompare(object expectedValue, object propertyValue, out int? compareValue)
        {
            compareValue = null;
            var propertyComparable = propertyValue as IComparable;
            var expectedComparable = expectedValue as IComparable;
            if (propertyComparable == null || expectedComparable == null) return false;

            compareValue = propertyComparable.CompareTo(expectedComparable);
            return true;
        }

        public enum StringCaseEnum
        {
            Lower, Upper
        }
    }
}