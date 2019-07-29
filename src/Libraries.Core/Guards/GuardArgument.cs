using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Guards
{
    public class GuardArgument<T>
    {
        private readonly GuardCondition<T> _guardCondition;
        private readonly string _methodName;
        private readonly string _rationale;

        public GuardArgument(GuardCondition<T> guardCondition, string methodName, string rationale)
        {
            _guardCondition = guardCondition;
            _methodName = methodName;
            _rationale = rationale;
        }

        public GuardConfiguration<T> Parameter<TValue>(TValue value, string name)
            where TValue: T
        {
            switch (_methodName)
            {
                case nameof(GuardCondition<T>.GreaterThan):
                    GuardCondition<T>.MustBeType<T, TValue>(value, _methodName, out var c1);
                    return _guardCondition.GreaterThan(c1, $"be > the parameter {name} ({value})", _rationale);
                default:
                    FulcrumAssert.Fail($"Unknown {nameof(GuardCondition<T>)} method name: {_methodName}");
                    break;
            }
            return _guardCondition.Config;
        }
    }
}