using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    public class ActivityInstance : ActivityInstanceCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
    {
        public string Id { get; set; }
        public string Etag { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public string AsyncRequestId { get; set; }

        public bool HasCompleted { get; set; }

        public string ResultAsJson { get; set; }

        public string ExceptionType { get; set; }

        public string ExceptionMessage { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
        }
    }

    public class ActivityInstanceCreate : IValidatable
    {
        public string WorkflowInstanceId { get; set; }
        public string ActivityVersionId { get; set; }
        public string ParentActivityInstanceId { get; set; }
        public int? Iteration { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(ActivityVersionId, nameof(ActivityVersionId), errorLocation);
            if (Iteration.HasValue) FulcrumValidate.IsGreaterThanOrEqualTo(1, Iteration.Value, nameof(Iteration), errorLocation);
        }
    }
}