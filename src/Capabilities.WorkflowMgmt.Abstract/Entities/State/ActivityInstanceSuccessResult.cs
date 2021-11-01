using Nexus.Link.Libraries.Core.Assert;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State
{
    public class ActivityInstanceSuccessResult : IValidatable
    {
        public string ResultAsJson { get; set; }

        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(ResultAsJson, nameof(ResultAsJson), errorLocation);
        }
    }
}