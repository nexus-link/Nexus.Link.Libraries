using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Guards
{
    public class ServiceContractGuard : GenericGuard<FulcrumServiceContractException>
    {
        public GuardConfiguration<T> Parameter<T>(T value, string name)
        {
            return ThrowIfUnsuccessful(value, $"parameter {name}");
        }
    }
    public class ResourceContractGuard : GenericGuard<FulcrumResourceException>
    {
        public GuardConfiguration<T> Parameter<T>(T value, string name)
        {
            return ThrowIfUnsuccessful(value, $"parameter {name}");
        }
    }

    public class InternalContractGuard : GenericGuard<FulcrumContractException>
    {
        public GuardConfiguration<T> Parameter<T>(T value, string name)
        {
            return ThrowIfUnsuccessful(value, $"parameter {name}");
        }
    }

    public class AssertGuard : GenericGuard<FulcrumAssertionFailedException>
    {
        public GuardConfiguration<T> Value<T>(T value, string description = null)
        {
            return ThrowIfUnsuccessful(value, description ?? "the value");
        }
    }

    public class ValidateGuard : GenericGuard<ValidationException>
    {
        public GuardConfiguration<T> Property<T>(T value, string name)
        {
            return ThrowIfUnsuccessful(value, $"property {name}");
        }
    }

    public abstract class GenericGuard<TException>
        where TException : FulcrumException, new()
    {
        public SimpleGuardCondition<object> Is { get; }

        public GenericGuard()
        {
            Is =  new SimpleGuardCondition<object>(new GuardConfiguration<object>(null, null));
        }

        protected GuardConfiguration<T> ThrowIfUnsuccessful<T>(T value, string description)
        {
            try
            {
                return new GuardConfiguration<T>(value, description);
            }
            catch (GuardException e)
            {
                var fulcrumException = new TException { TechnicalMessage = e.Message };
                throw fulcrumException;
            }
        }
    }

    public class GuardException : Exception
    {
        public GuardException(string message) :base(message)
        {
        }
    }
}
