using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Assert
{
    /// <summary>
    /// A generic class for verifying method contracts. Generic in the meaning that a parameter says what exception that should be thrown when a requirement doesn't hold.
    /// </summary>
    internal static class GenericContract<TException>
        where TException : FulcrumException
    {
        /// <summary>
        /// Verify that <paramref name="expression"/> return true, when applied to <paramref name="parameterValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void Require<TParameter>(TParameter parameterValue,
            Expression<Func<TParameter, bool>> expression, string parameterName)
        {
            var message = GetErrorMessageIfFalse(parameterValue, expression, parameterName);
            MaybeThrowException(message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not null.
        /// </summary>
        [StackTraceHidden]
        [ContractAnnotation("parameterValue:null => halt")]
        public static void RequireNotNull<TParameter>(TParameter parameterValue, string parameterName, string customMessage = null)
        {
            var message = GetErrorMessageIfNull(parameterValue, parameterName, customMessage);
            MaybeThrowException(message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not the default value for this type.
        /// </summary>
        [StackTraceHidden]
        public static void RequireNotDefaultValue<TParameter>(TParameter parameterValue, string parameterName, string customMessage = null)
        {
            var message = GetErrorMessageIfDefaultValue(parameterValue, parameterName, customMessage);
            MaybeThrowException(message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not null, not empty and contains other characters than white space.
        /// </summary>
        [StackTraceHidden]
        [ContractAnnotation("parameterValue:null => halt")]
        public static void RequireNotNullOrWhiteSpace(string parameterValue, string parameterName, string customMessage = null)
        {
            var message = GetErrorMessageIfNullOrWhiteSpace(parameterValue, parameterName, customMessage);
            MaybeThrowException(message);
        }

        /// <summary>
        /// Verify that <paramref name="mustBeTrue"/> really is true.
        /// </summary>
        [StackTraceHidden]
        public static void Require(bool mustBeTrue, string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            var m = GetErrorMessageIfFalse(mustBeTrue, message);
            MaybeThrowException(m);
        }

        /// <summary>
        /// Always fail, with the given <paramref name="message"/>.
        /// </summary>
        [StackTraceHidden]
        [ContractAnnotation("=> halt")]
        public static void Fail(string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            GenericBase<TException>.ThrowException(message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireAreEqual<T>(T expectedValue, T parameterValue, string parameterName, string customMessage = null)
        {
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must be equal to ({expectedValue}).";
            Require(Equals(expectedValue, parameterValue), message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireAreNotEqual<T>(T expectedValue, T parameterValue, string parameterName, string customMessage = null)
        {
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must not be equal to ({expectedValue}).";
            Require(!Equals(expectedValue, parameterValue), message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is less than to <paramref name="greaterValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireLessThan<T>(T greaterValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(greaterValue, nameof(greaterValue));
            InternalContract.RequireNotNull(parameterValue, nameof(parameterValue));
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must be less than ({greaterValue}).";
            Require(parameterValue.CompareTo(greaterValue) < 0, message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is less than or equal to <paramref name="greaterOrEqualValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireLessThanOrEqualTo<T>(T greaterOrEqualValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(greaterOrEqualValue, nameof(greaterOrEqualValue));
            InternalContract.RequireNotNull(parameterValue, nameof(parameterValue));
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must be less than or equal to ({greaterOrEqualValue}).";
            Require(parameterValue.CompareTo(greaterOrEqualValue) <= 0, message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is greater than <paramref name="lesserValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireGreaterThan<T>(T lesserValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(lesserValue, nameof(lesserValue));
            InternalContract.RequireNotNull(parameterValue, nameof(parameterValue));
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must be greater than ({lesserValue}).";
            Require(parameterValue.CompareTo(lesserValue) > 0, message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is greater than or equal to <paramref name="lesserOrEqualValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireGreaterThanOrEqualTo<T>(T lesserOrEqualValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(lesserOrEqualValue, nameof(lesserOrEqualValue));
            InternalContract.RequireNotNull(parameterValue, nameof(parameterValue));
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must be greater than or equal to ({lesserOrEqualValue}).";
            Require(parameterValue.CompareTo(lesserOrEqualValue) >= 0, message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireMatchesRegExp(string regularExpression, string parameterValue, string parameterName, string customMessage = null)
        {
            InternalContract.RequireNotNull(regularExpression, nameof(regularExpression));
            InternalContract.RequireNotNull(parameterValue, nameof(parameterValue));
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must match regular expression ({regularExpression}).";
            Require(Regex.IsMatch(parameterValue, regularExpression), message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireMatchesNotRegExp(string regularExpression, string parameterValue, string parameterName, string customMessage = null)
        {
            InternalContract.RequireNotNull(regularExpression, nameof(regularExpression));
            InternalContract.RequireNotNull(parameterValue, nameof(parameterValue));
             if (customMessage == null) InternalContract.RequireNotNull(parameterName, nameof(parameterName));
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must not match regular expression ({regularExpression}).";
            Require(!Regex.IsMatch(parameterValue, regularExpression), message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is null or has one of the values in <paramref name="enumerationType"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireInEnumeration(Type enumerationType, string parameterValue, string parameterName, string customMessage = null)
        {
            InternalContract.RequireNotNull(enumerationType, nameof(enumerationType));
            InternalContract.Require(enumerationType.IsEnum, $"Parameter {nameof(enumerationType)} must be of type enum.");
            if (parameterValue == null) return;
            var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must represent one of the enumeration values for ({enumerationType.FullName}).";
            Require(Enum.IsDefined(enumerationType, parameterValue), message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is null or in JSON format.
        /// </summary>
        [StackTraceHidden]
        public static void RequireJson(string parameterValue, string parameterName, string customMessage = null)
        {
            if (parameterValue == null) return;
            try
            {
                JToken.Parse(parameterValue);
            }
            catch (Exception e)
            {
                var message = customMessage ?? $"ContractViolation: {parameterName} ({parameterValue}) must be null or in JSON format: {e.Message}";
                Require(false, message);
            }
        }

        /// <summary>
        /// If <paramref name="parameterValue"/> is not null, then call the Validate() method of that type.
        /// </summary>
        [StackTraceHidden]
        public static void RequireValidated(object parameterValue, string parameterName, string customMessage = null)
        {
            var result = Validation.Validate(parameterValue, null);
            if (!result.IsValid)
            {
                GenericBase<TException>.ThrowException(customMessage ?? $"{result.Path}.{result.Message}");
            }
            if (!(parameterValue is IValidatable validatable)) return;
            try
            {
                validatable.Validate(null, parameterValue.GetType().Name);
            }
            catch (ValidationException e)
            {
                GenericBase<TException>.ThrowException(customMessage ?? $"ContractViolation: Validation failed for {parameterName} ({e.Message}).");
            }
        }

        private static string GetErrorMessageIfFalse<T>(T parameterValue, Expression<Func<T, bool>> requirementExpression, string parameterName)
        {
            if (requirementExpression.Compile()(parameterValue)) return null;

            var condition = requirementExpression.Body.ToString();
            condition = condition.Replace(requirementExpression.Parameters.First().Name, parameterName);
            return $"Contract violation: {parameterName} ({parameterValue}) is required to fulfil {condition}.";
        }

        private static string GetErrorMessageIfNull<T>(T parameterValue, string parameterName, string customMessage)
        {
            return parameterValue != null ? null : customMessage ?? $"Contract violation: {parameterName} must not be null.";
        }

        private static string GetErrorMessageIfDefaultValue<T>(T parameterValue, string parameterName, string customMessage)
        {
            if (!Equals(parameterValue, default(T))) return null;
            return customMessage ?? $"Contract violation: {parameterName} must not be null.";
        }

        private static string GetErrorMessageIfNullOrWhiteSpace(string parameterValue, string parameterName, string customMessage)
        {
            if (!string.IsNullOrWhiteSpace(parameterValue)) return null;
            var value = parameterValue == null ? "null" : $"\"{parameterValue}\"";
            return customMessage ?? $"Contract violation: {parameterName} ({value}) must not be null, empty or whitespace.";
        }
        private static string GetErrorMessageIfFalse(bool mustBeTrue, string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            return mustBeTrue ? null : message;
        }

        [StackTraceHidden]
        public static void MaybeThrowException(string message)
        {
            if (message == null) return;
            GenericBase<TException>.ThrowException(message);
        }
    }
}
