﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Assert
{
    /// <summary>
    /// A class for verifying method contracts. Will throw <see cref="FulcrumContractException"/> if the contract is broken.
    /// </summary>
    public static class InternalContract
    {
        /// <summary>
        /// Verify that <paramref name="expression"/> return true, when applied to <paramref name="parameterValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void Require<TParameter>(TParameter parameterValue,
            Expression<Func<TParameter, bool>> expression, string parameterName)
        {
            GenericContract<FulcrumContractException>.Require(parameterValue, expression, parameterName);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not null.
        /// </summary>
        [StackTraceHidden]
        
        public static void RequireNotNull(object parameterValue, string parameterName, string customMessage = null)
        {
            GenericContract<FulcrumContractException>.RequireNotNull(parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not the default parameterValue for this type.
        /// </summary>
        [StackTraceHidden]
        public static void RequireNotDefaultValue<TParameter>(TParameter parameterValue, string parameterName, string customMessage = null)
        {
            GenericContract<FulcrumContractException>.RequireNotDefaultValue(parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not null, not empty and contains other characters than white space.
        /// </summary>
        [StackTraceHidden]
        
        public static void RequireNotNullOrWhiteSpace(string parameterValue, string parameterName, string customMessage = null)
        {
            GenericContract<FulcrumContractException>.RequireNotNullOrWhiteSpace(parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// If <paramref name="parameterValue"/> is not null, then call the FulcrumValidate() method of that type.
        /// </summary>
        [StackTraceHidden]
        public static void RequireValidated(object parameterValue, string parameterName, string customMessage = null)
        {
            GenericContract<FulcrumContractException>.RequireValidated(parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// If <paramref name="parameterValues"/> is not null, then call the Validate() method for each item.
        /// </summary>
        [StackTraceHidden]
        public static void RequireValidated(IEnumerable<object> parameterValues, string parameterName, string customMessage = null)
        {
            if (parameterValues == null) return;
            foreach (var parameterValue in parameterValues)
            {
                GenericContract<FulcrumContractException>.RequireValidated(parameterValue, parameterName, customMessage);
            }
        }

        /// <summary>
        /// Verify that <paramref name="mustBeTrue"/> really is true.
        /// </summary>
        [StackTraceHidden]
        public static void Require(bool mustBeTrue, string message)
        {
            RequireNotNullOrWhiteSpace(message, nameof(message));
            GenericContract<FulcrumContractException>.Require(mustBeTrue, message);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireAreEqual<T>(T expectedValue, T parameterValue, string parameterName, string customMessage = null)
        {
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireAreEqual(expectedValue, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is not equal to <paramref name="expectedValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireAreNotEqual<T>(T expectedValue, T parameterValue, string parameterName, string customMessage = null)
        {
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireAreNotEqual(expectedValue, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is less than to <paramref name="greaterValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireLessThan<T>(T greaterValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            RequireNotNull(greaterValue, nameof(greaterValue));
            RequireNotNull(parameterValue, nameof(parameterValue));
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireLessThan(greaterValue, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is less than or equal to <paramref name="greaterOrEqualValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireLessThanOrEqualTo<T>(T greaterOrEqualValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            RequireNotNull(greaterOrEqualValue, nameof(greaterOrEqualValue));
            RequireNotNull(parameterValue, nameof(parameterValue));
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireLessThanOrEqualTo(greaterOrEqualValue, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is greater than <paramref name="lesserValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireGreaterThan<T>(T lesserValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            RequireNotNull(lesserValue, nameof(lesserValue));
            RequireNotNull(parameterValue, nameof(parameterValue));
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireGreaterThan(lesserValue, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is greater than or equal to <paramref name="lesserOrEqualValue"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireGreaterThanOrEqualTo<T>(T lesserOrEqualValue, T parameterValue, string parameterName, string customMessage = null)
            where T : IComparable<T>
        {
            RequireNotNull(lesserOrEqualValue, nameof(lesserOrEqualValue));
            RequireNotNull(parameterValue, nameof(parameterValue));
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireGreaterThanOrEqualTo(lesserOrEqualValue, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is null or matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireMatchesRegExp(string regularExpression, string parameterValue, string parameterName, string customMessage = null)
        {
            RequireNotNullOrWhiteSpace(regularExpression, nameof(regularExpression));
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireMatchesRegExp(regularExpression, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or not matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireMatchesNotRegExp(string regularExpression, string value, string parameterName, string customMessage = null)
        {
            RequireNotNullOrWhiteSpace(regularExpression, nameof(regularExpression));
            GenericContract<FulcrumContractException>.RequireMatchesNotRegExp(regularExpression, value, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is null or matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        [Obsolete("Use RequireMatchesRegExp. Obsolete warning since 2021-05-03.")]
        public static void MatchesRegExp(string regularExpression, string parameterValue, string parameterName, string customMessage = null)
        {
            RequireNotNullOrWhiteSpace(regularExpression, nameof(regularExpression));
            if (customMessage == null) RequireNotNull(parameterName, nameof(parameterName));
            GenericContract<FulcrumContractException>.RequireMatchesRegExp(regularExpression, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="value"/> is null or not matches the regular expression <paramref name="regularExpression"/>.
        /// </summary>
        [StackTraceHidden]
        [Obsolete("Use RequireMatchesNotRegExp. Obsolete warning since 2021-05-03.")]
        public static void MatchesNotRegExp(string regularExpression, string value, string parameterName, string customMessage = null)
        {
            RequireNotNullOrWhiteSpace(regularExpression, nameof(regularExpression));
            GenericContract<FulcrumContractException>.RequireMatchesNotRegExp(regularExpression, value, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is null or has one of the values in <paramref name="enumerationType"/>.
        /// </summary>
        [StackTraceHidden]
        public static void RequireInEnumeration(Type enumerationType, string parameterValue, string parameterName, string customMessage = null)
        {
            RequireNotNull(enumerationType, nameof(enumerationType));
            Require(enumerationType.IsEnum, $"Parameter {nameof(enumerationType)} must be of type enum.");
            GenericContract<FulcrumContractException>.RequireInEnumeration(enumerationType, parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Verify that <paramref name="parameterValue"/> is null or in JSON format.
        /// </summary>
        [StackTraceHidden]
        public static void RequireJson(string parameterValue, string parameterName, string customMessage = null)
        {
            GenericContract<FulcrumContractException>.RequireJson(parameterValue, parameterName, customMessage);
        }

        /// <summary>
        /// Always fail, with the given <paramref name="message"/>.
        /// </summary>
        [StackTraceHidden]
        
        public static void Fail(string message)
        {
            RequireNotNull(message, nameof(message));
            GenericContract<FulcrumContractException>.Fail(message);
        }
    }
}
