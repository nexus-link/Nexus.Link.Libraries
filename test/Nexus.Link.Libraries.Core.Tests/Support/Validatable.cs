using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Tests.Support
{
    internal class Validatable : IValidatable
    {
        public string Name { get; set; }

        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Name, propertyPath + "." + nameof(Name), errorLocation);
        }
    }
}
