using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Guards
{
    public class SimpleGuardCondition<T>
    {
        private readonly GuardConfiguration<T> _config;

        public SimpleGuardCondition(GuardConfiguration<T> config)
        {
            _config = config;
        }

        public GuardConfiguration<T> True(bool mustBeTrue, string completeMessage)
        {
            if (mustBeTrue) return _config;
            throw new GuardException(completeMessage);
        }

        public GuardConfiguration<T> False(bool mustBeFalse, string completeMessage)
        {
            if (!mustBeFalse) return _config;
            throw new GuardException(completeMessage);
        }
    }
}