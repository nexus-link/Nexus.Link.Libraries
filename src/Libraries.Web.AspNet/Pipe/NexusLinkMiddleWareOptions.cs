using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Options;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    // TODO: Move all features into a class, Feature
    public class NexusLinkMiddlewareOptions : IValidatable
    {
        public MiddlewareFeatures Features = new MiddlewareFeatures();

        public MiddlewareCallbacks Callbacks = new MiddlewareCallbacks();
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }

    public class MiddlewareCallbacks
    {
    }
}