using Nexus.Link.Libraries.Core.Assert;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State
{
    public class ActivityInstanceFailedResult : IValidatable
    {
        public ActivityExceptionCategoryEnum? ExceptionCategory { get; set; }
        public string ExceptionTechnicalMessage { get; set; }
        public string ExceptionFriendlyMessage { get; set; }

        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(ExceptionTechnicalMessage, nameof(ExceptionTechnicalMessage), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(ExceptionFriendlyMessage, nameof(ExceptionFriendlyMessage), errorLocation);
        }
    }
}