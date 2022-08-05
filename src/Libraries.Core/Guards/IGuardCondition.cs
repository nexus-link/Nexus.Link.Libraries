using System;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Guards
{
    public static class GuardExtensions
    {
        public static GuardConfiguration<T> True<T>(this GuardCondition<T> guard, bool assertion, string mustMessage, bool showValue, string rationale = null)
        {
            return guard.MaybeThrowException(assertion, mustMessage, showValue, rationale);
        }

        public static GuardConfiguration<T> Null<T>(this GuardCondition<T> guard, string rationale = null)
        {
            var success = guard.CalculateSuccess(guard.Value == null);
            return guard.MaybeThrowException(success, $"be null", !guard.IsNegationActive, rationale);
        }

        public static GuardConfiguration<T> NullOrWhitespace<T>(this GuardCondition<T> guard, string rationale = null)
        {
            guard.MustBeSubclassOf<T, string>(nameof(NullOrWhitespace), out var value);
            var success = guard.CalculateSuccess(string.IsNullOrWhiteSpace(value));
            return guard.MaybeThrowException(success, $"be null, empty or whitespace", true, rationale);
        }

        public static GuardConfiguration<T> GreaterThan<T, TValue>(this GuardCondition<T> guard, TValue otherValue, string rationale = null)
            where TValue : T, IComparable
        {
            return guard.GreaterThan<T,TValue>(otherValue, $"be > \"{otherValue}\"", rationale);
        }

        internal static GuardConfiguration<T> GreaterThan<T, TValue>(this GuardCondition<T> guard, TValue otherValue, string mustMessage, string rationale)
            where TValue : T
        {
            guard.MustBeType<T, IComparable>(nameof(GreaterThan), out var thisValue);
            var success = guard.CalculateSuccess(thisValue.CompareTo(otherValue) > 0);
            return guard.MaybeThrowException(success, mustMessage, true, rationale);
        }

        public static GuardConfiguration<T> GreaterThanOrEqualTo<T, TValue>(this GuardCondition<T> guard, TValue otherValue, string rationale = null)
            where TValue : T, IComparable
        {
            guard.MustBeType<T, IComparable>(nameof(GreaterThanOrEqualTo), out var thisValue);
            var success = guard.CalculateSuccess(thisValue.CompareTo(otherValue) > 0);
            return guard.MaybeThrowException(success, $"be >= \"{otherValue}\"", true, rationale);
        }

        public static GuardArgument<T> GreaterThan<T>(this GuardCondition<T> guard, string rationale = null)
        {
            guard.MustBeType<T, IComparable>(nameof(GreaterThan));
            return new GuardArgument<T>(guard, nameof(GreaterThan), rationale);
        }

        public static GuardConfiguration<T> AssignableTo<T, TExpected>(this GuardCondition<T> guard, out TExpected o, string rationale = null)
        {
            var success = guard.CalculateSuccess(typeof(TExpected).IsAssignableFrom(typeof(T)));
            o = (TExpected)(object)guard.Value;
            return guard.MaybeThrowExceptionBasedOnType(success, $"be assignable to the type {typeof(TExpected).FullName}", true, rationale);
        }

        public static GuardConfiguration<T> AssignableTo<T, TExpected>(this GuardCondition<T> guard, string rationale = null)
        {
            return guard.AssignableTo<T, TExpected>(out _, rationale);
        }

        public static GuardConfiguration<T> Valid<T>(this GuardCondition<T> guard)
        {
            if (guard.Value == null) return guard.Config;
            if (!(guard.Value is IValidatable validatable)) return guard.Config;
            try
            {
                // TODO: Refactor Validate to accept string customMessage = null, int lineNumber = 0, string filePath = "", string memberName = ""
                validatable.Validate("No location");
                return guard.Config;
            }
            catch (ValidationException e)
            {
                return guard.MaybeThrowException(false, $"be valid according to the validation requirements of its type {guard.ValueTypeOrGenericType().FullName}: {e.Message}",
                    true, null);
            }
        }

        private static GuardConfiguration<T> MaybeThrowExceptionBasedOnType<T>(this GuardCondition<T> guard, bool success, string mustMessage, bool showValue, string rationale)
        {
            var showValueMessage = showValue ? $" with type {guard.ValueTypeOrGenericType().FullName}" : "";
            return MaybeThrowException(guard, success, mustMessage, showValueMessage, rationale);
        }

        private static GuardConfiguration<T> MaybeThrowException<T>(this GuardCondition<T> guard, bool success, string mustMessage, bool showValue, string rationale)
        {
            var showValueMessage = showValue ? $" ({guard.TheValueAsStringOrTheWordNull()})" : "";
            return MaybeThrowException(guard, success, mustMessage, showValueMessage, rationale);
        }

        private static GuardConfiguration<T> MaybeThrowException<T>(this GuardCondition<T> guard, bool success, string mustMessage, string showValueMessage, string rationale)
        {
            if (success) return guard.Config;
            var message = $"{guard.Config.FormattedDescription(true)}" +
                          showValueMessage +
                          $" must {guard.MaybeTheWordNot}{mustMessage}." +
                          (rationale == null ? $" {rationale}" : "");
            throw new GuardException(message);
        }

        private static bool CalculateSuccess<T>(this GuardCondition<T> guard, bool result) => guard.IsNegationActive ? !result : result;


        private static void MustBeSubclassOf<T, TParent>(this GuardCondition<T> guard, string methodName, out TParent value)
            where TParent : class
        {
            MustBeSubclassOf(guard.Value, methodName, out value);
        }

        private static void MustBeSubclassOf<TChild, TParent>(TChild childValue, string methodName, out TParent parentValue)
            where TParent : class
        {
            parentValue = childValue as TParent;
            if (parentValue != null) return;
            throw new Exception(
                $"The type {typeof(TChild).FullName} was expected to be a subclass of type {typeof(TParent).FullName} to call {methodName}.");
        }

        private static void MustBeType<TActual, TExpected>(TActual childValue, string methodName, out TExpected value)
        {
            try
            {
                value = (TExpected)(object)childValue;
            }
            catch (Exception)
            {
                throw new Exception(
                    $"The type {typeof(TActual).FullName} was expected to be of type {typeof(TExpected).FullName} to call {methodName}.");
            }
        }

        private static void MustBeType<T, TExpected>(this GuardCondition<T> guard, string methodName, out TExpected value)
        {
            MustBeType(guard.Value, methodName, out value);
        }

        private static void MustBeType<T, TExpected>(this GuardCondition<T> guard, string methodName)
        {
            MustBeType<T, TExpected>(guard.Value, methodName, out _);
        }
    }
}