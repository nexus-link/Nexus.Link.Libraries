using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    public class ActivityVersion : ActivityVersionCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
    {
        public string Id { get; set; }
        public string Etag { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
        }
    }

    public class ActivityVersionCreate : IValidatable
    {
        public string WorkflowVersionId { get; set; }
        public string ActivityFormId { get; set; }
        public int Position { get; set; }
        public string ParentActivityVersionId { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(ActivityFormId, nameof(ActivityFormId), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(1, Position, nameof(Position), errorLocation);
        }
    }
}