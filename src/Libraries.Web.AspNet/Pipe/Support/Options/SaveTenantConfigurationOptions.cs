using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options
{
    public class SaveTenantConfigurationOptions : Feature, IValidatable
    {
        /// <summary>
        ///  TODO
        /// </summary>
        public ILeverServiceConfiguration ServiceConfiguration { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Enabled)
            {
                FulcrumValidate.IsNotNull(ServiceConfiguration, nameof(ServiceConfiguration), errorLocation);
            }
        }
    }
}