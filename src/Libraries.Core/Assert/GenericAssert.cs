using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Assert
{ 
    /// <summary>
    /// A generic class for asserting things that the programmer thinks is true. Generic in the meaning that a parameter says what exception that should be thrown when an assumption is false.
    /// </summary>
    internal static class GenericAssert<TException>
        where TException : FulcrumException
    {
        /// <summary>
        /// Will always fail. Used in parts of the errorLocation where we should never end up. E.g. a default case in a switch statement where all cases should be covered, so we should never end up in the default case.
        /// </summary>
        /// <param name="errorLocation">A unique errorLocation for this exact assertion.</param>
        /// <param name="message">A message that documents/explains this failure. This message should normally start with "Expected ...".</param>
        [StackTraceHidden]
        public static void Fail(string errorLocation, string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            GenericBase<TException>.ThrowException(message, errorLocation);
        }
        /// <summary>
        /// Will always fail. Used in parts of the errorLocation where we should never end up. E.g. a default case in a switch statement where all cases should be covered, so we should never end up in the default case.
        /// </summary>
        /// <param name="message">A message that documents/explains this failure. This message should normally start with "Expected ...".</param>
        [StackTraceHidden]
        public static void Fail(string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            GenericBase<TException>.ThrowException(message);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is true.
        /// </summary>
        [StackTraceHidden]
        public static void IsTrue(bool value, string errorLocation = null, string customMessage = null)
        {
            if (value) return;
            GenericBase<TException>.ThrowException(customMessage ?? "Expected value to be true.", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null.
        /// </summary>
        [StackTraceHidden]
        public static void IsNull(object value, string errorLocation = null, string customMessage = null)
        {
            if (value == null) return;
            GenericBase<TException>.ThrowException(customMessage ?? $"Expected value ({value}) to be null.", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is not null.
        /// </summary>
        [StackTraceHidden]
        public static void IsNotNull(object value, string errorLocation = null, string customMessage = null)
        {
            if (value != null) return;
            GenericBase<TException>.ThrowException(customMessage ?? "Did not expect value to be null.", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is not the default value for that type.
        /// </summary>
        [StackTraceHidden]
        public static void IsNotDefaultValue<T>(T value, string errorLocation = null, string customMessage = null)
        {
            if (!value.Equals(default(T))) return;
            GenericBase<TException>.ThrowException(customMessage ?? $"Did not expect value to be default value ({default(T)}).", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is not null, not empty and has other characters than white space.
        /// </summary>
        [StackTraceHidden]
        public static void IsNotNullOrWhiteSpace(string value, string errorLocation = null, string customMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(value)) return;
            GenericBase<TException>.ThrowException(customMessage ?? $"Did not expect value ({value}) to be null, empty or only contain whitespace.", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void AreEqual(object expectedValue, object actualValue, string errorLocation = null, string customMessage = null)
        {
            if (Equals(expectedValue, actualValue)) return;
            GenericBase<TException>.ThrowException(customMessage ?? $"Expected ({actualValue}) to be equal to ({expectedValue}).", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is not equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void AreNotEqual(object expectedValue, object actualValue, string errorLocation = null, string customMessage = null)
        {
            if (!Equals(expectedValue, actualValue)) return;
            GenericBase<TException>.ThrowException(customMessage ?? $"Expected ({actualValue}) to not be equal to ({expectedValue}).", errorLocation);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is less than to <paramref name="greaterValue"/>.
        /// </summary>
        public static void IsLessThan<T>(T greaterValue, T actualValue, string errorLocation = null, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(greaterValue, nameof(greaterValue));
            InternalContract.RequireNotNull(actualValue, nameof(actualValue));
            var message = customMessage ?? $"Expected ({actualValue}) to be less than ({greaterValue}).";
            IsTrue(actualValue.CompareTo(greaterValue) < 0, errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is less than or equal to <paramref name="greaterOrEqualValue"/>.
        /// </summary>
        public static void IsLessThanOrEqualTo<T>(T greaterOrEqualValue, T actualValue, string errorLocation = null, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(greaterOrEqualValue, nameof(greaterOrEqualValue));
            InternalContract.RequireNotNull(actualValue, nameof(actualValue));
            var message = customMessage ?? $"Expected ({actualValue}) to be less than or equal to ({greaterOrEqualValue}).";
            IsTrue(actualValue.CompareTo(greaterOrEqualValue) <= 0, errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is greater than <paramref name="lesserValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void IsGreaterThan<T>(T lesserValue, T actualValue, string errorLocation = null, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(lesserValue, nameof(lesserValue));
            InternalContract.RequireNotNull(actualValue, nameof(actualValue));
            var message = customMessage ?? $"Expected ({actualValue}) to be greater than ({lesserValue}).";
            IsTrue(actualValue.CompareTo(lesserValue) > 0, errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is greater than or equal to <paramref name="lesserOrEqualValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void IsGreaterThanOrEqualTo<T>(T lesserOrEqualValue, T actualValue, string errorLocation = null, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(lesserOrEqualValue, nameof(lesserOrEqualValue));
            InternalContract.RequireNotNull(actualValue, nameof(actualValue));
            var message = customMessage ?? $"Expected ({actualValue}) to be greater than or equal to ({lesserOrEqualValue}).";
            IsTrue(actualValue.CompareTo(lesserOrEqualValue) >= 0, errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void MatchesRegExp(string regularExpression, string value, string errorLocation = null, string customMessage = null)
        {
            if (value == null) return;
            InternalContract.RequireNotNull(regularExpression, nameof(regularExpression));
            var message = customMessage ?? $"Expected ({value}) to match regular expression ({regularExpression}).";
            IsTrue(Regex.IsMatch(value, regularExpression), errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void MatchesNotRegExp(string regularExpression, string value, string errorLocation = null, string customMessage = null)
        {
            if (value == null) return;
            InternalContract.RequireNotNull(regularExpression, nameof(regularExpression));
            var message = customMessage ?? $"Expected ({value}) to not match regular expression ({regularExpression}).";
            IsTrue(!Regex.IsMatch(value, regularExpression), errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or has one of the values in <paramref name="enumerationType"/>.
        /// </summary>
        [StackTraceHidden]
        [Obsolete("Please use IsInEnumeration(). Obsolete since 2022-01-26.")]
        public static void InEnumeration(Type enumerationType, string value, string errorLocation = null, string customMessage = null)
        {
            IsInEnumeration(enumerationType, value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or has one of the values in <paramref name="enumerationType"/>.
        /// </summary>
        [StackTraceHidden]
        public static void IsInEnumeration(Type enumerationType, string value, string errorLocation = null, string customMessage = null)
        {
            InternalContract.RequireNotNull(enumerationType, nameof(enumerationType));
            InternalContract.Require(enumerationType.IsEnum, $"Parameter {nameof(enumerationType)} must be of type enum.");
            if (value == null) return;
            var message = customMessage ?? $"Expected  ({value}) to represent one of the enumeration values for ({enumerationType.FullName}).";
            IsTrue(Enum.IsDefined(enumerationType, value), errorLocation, message);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> null or a JSON expression.
        /// </summary>
        [StackTraceHidden]
        public static void IsJson(string value, string errorLocation = null, string customMessage = null)
        {
            if (value == null) return;
            try
            {
                JToken.Parse(value);
            }
            catch (Exception e)
            {
                var message = customMessage ?? $"Expected  ({value}) be a JSON expression: {e.Message}";
                Fail(errorLocation, message);
            }
        }

        /// <summary>
        /// If <paramref name="value"/> is not null, then call the Validate() method of that type.
        /// </summary>
        [StackTraceHidden]
        public static void IsValidated(object value, string customMessage = null)
        {
            if (value == null) return;
            if (!(value is IValidatable validatable)) return;
            try
            {
                validatable.Validate(null, value.GetType().Name);
            }
            catch (ValidationException e)
            {
                GenericBase<TException>.ThrowException($"Expected validation to pass ({e.Message}).");
            }
        }
    }
}
