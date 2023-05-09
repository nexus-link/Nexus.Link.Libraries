using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Assert
{
    /// <summary>
    /// A class for asserting things that the programmer thinks is true. Works both as documentation and as verification that the programmers assumptions holds.
    /// </summary>
    public static class FulcrumAssert
    {
        /// <summary>
        /// Will always fail. Used in parts of the errorLocation where we should never end up. E.g. a default case in a switch statement where all cases should be covered, so we should never end up in the default case.
        /// </summary>
        /// <param name="errorLocation">A unique errorLocation for this exact assertion. </param>
        /// <param name="message">A message that documents/explains this failure. This message should normally start with "Expected ...".</param>
        [StackTraceHidden]

        public static void Fail(string errorLocation, string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(errorLocation, nameof(errorLocation));
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            GenericAssert<FulcrumAssertionFailedException>.Fail(errorLocation, message);
        }
        /// <summary>
        /// Will always fail. Used in parts of the errorLocation where we should never end up. E.g. a default case in a switch statement where all cases should be covered, so we should never end up in the default case.
        /// </summary>
        /// <param name="message">A message that documents/explains this failure. This message should normally start with "Expected ...".</param>
        [StackTraceHidden]

        public static void Fail(string message)
        {
            InternalContract.RequireNotNullOrWhiteSpace(message, nameof(message));
            GenericAssert<FulcrumAssertionFailedException>.Fail(null, message);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is true.
        /// </summary>
        [StackTraceHidden]
        public static void IsTrue(bool value, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.IsTrue(value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null.
        /// </summary>
        [StackTraceHidden]
        public static void IsNull(object value, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.IsNull(value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is not null.
        /// </summary>

        [StackTraceHidden]
        
        public static void IsNotNull(object value, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.IsNotNull(value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is not the default value for that type.
        /// </summary>
        [StackTraceHidden]
        public static void IsNotDefaultValue<T>(T value, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.IsNotDefaultValue(value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is not null, not empty and contains other characters than white space.
        /// </summary>
        [StackTraceHidden]
        
        public static void IsNotNullOrWhiteSpace(string value, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.IsNotNullOrWhiteSpace(value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void AreEqual(object expectedValue, object actualValue, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.AreEqual(expectedValue, actualValue, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is not equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void AreNotEqual(object expectedValue, object actualValue, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.AreNotEqual(expectedValue, actualValue, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is less than to <paramref name="greaterValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void IsLessThan<T>(T greaterValue, T actualValue, string errorLocation = null, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(greaterValue, nameof(greaterValue));
            InternalContract.RequireNotNull(actualValue, nameof(actualValue));
            GenericAssert<FulcrumAssertionFailedException>.IsLessThan(greaterValue, actualValue, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="actualValue"/> is less than or equal to <paramref name="greaterOrEqualValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void IsLessThanOrEqualTo<T>(T greaterOrEqualValue, T actualValue, string errorLocation = null, string customMessage = null)
            where T : IComparable<T>
        {
            InternalContract.RequireNotNull(greaterOrEqualValue, nameof(greaterOrEqualValue));
            InternalContract.RequireNotNull(actualValue, nameof(actualValue));
            GenericAssert<FulcrumAssertionFailedException>.IsLessThanOrEqualTo(greaterOrEqualValue, actualValue, errorLocation, customMessage);
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
            GenericAssert<FulcrumAssertionFailedException>.IsGreaterThan(lesserValue, actualValue, errorLocation, customMessage);
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
            GenericAssert<FulcrumAssertionFailedException>.IsGreaterThanOrEqualTo(lesserOrEqualValue, actualValue, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void MatchesRegExp(string regularExpression, string value, string errorLocation = null, string customMessage = null)
        {
            InternalContract.RequireNotNullOrWhiteSpace(regularExpression, nameof(regularExpression));
            GenericAssert<FulcrumAssertionFailedException>.MatchesRegExp(regularExpression, value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or not matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void MatchesNotRegExp(string regularExpression, string value, string errorLocation = null, string customMessage = null)
        {
            InternalContract.RequireNotNullOrWhiteSpace(regularExpression, nameof(regularExpression));
            GenericAssert<FulcrumAssertionFailedException>.MatchesNotRegExp(regularExpression, value, errorLocation, customMessage);
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
            GenericAssert<FulcrumAssertionFailedException>.IsInEnumeration(enumerationType, value, errorLocation, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> null or a JSON expression.
        /// </summary>
        [StackTraceHidden]
        public static void IsJson(string value, string errorLocation = null, string customMessage = null)
        {
            GenericAssert<FulcrumAssertionFailedException>.IsJson(value, errorLocation, customMessage);
        }

        /// <summary>
        /// Call the Validate() method for <paramref name="value"/>
        /// </summary>
        [StackTraceHidden]
        public static void IsValidated(object value, string errorLocation = null)
        {
            if (value == null) return;
            GenericAssert<FulcrumAssertionFailedException>.IsValidated(value, errorLocation);
        }

        /// <summary>
        /// Call the Validate() method for each item in <paramref name="values"/>
        /// </summary>
        [StackTraceHidden]
        public static void IsValidated(IEnumerable<object> values, string errorLocation = null)
        {
            if (values == null) return;
            foreach (var value in values)
            {
                IsValidated(value, errorLocation);
            }
        }
    }
}
