using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    internal class DelegateState
    {
        private readonly OneValueProvider<bool> _hasStartedValueProvider;

        public DelegateState(string delegateName)
        {
            _hasStartedValueProvider =
                new OneValueProvider<bool>(FulcrumApplication.Context.ValueProvider, delegateName + ".HasStarted");
        }

        public bool HasStarted
        {
            get => _hasStartedValueProvider.GetValue();
            set => _hasStartedValueProvider.SetValue(value);
        }
    }
}
