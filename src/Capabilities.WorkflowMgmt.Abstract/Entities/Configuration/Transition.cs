using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    public class Transition : TransitionCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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

    public class TransitionCreate : TransitionUnique, IValidatable
    {
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            if (string.IsNullOrWhiteSpace(FromActivityVersionId))
            {
                FulcrumValidate.IsTrue(!string.IsNullOrWhiteSpace(ToActivityVersionId), errorLocation, $"One of {nameof(FromActivityVersionId)} and {nameof(ToActivityVersionId)} must be not null.");
            }
        }
    }

    public class TransitionUnique
    {
        public string WorkflowVersionId { get; set; }
        public string FromActivityVersionId { get; set; }
        public string ToActivityVersionId { get; set; }
    }
}