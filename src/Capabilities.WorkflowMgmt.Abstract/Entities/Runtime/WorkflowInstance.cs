using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    /// <summary>
    /// Information about an instance of a <see cref="WorkflowVersion"/>.
    /// </summary>
    public class WorkflowInstance : WorkflowInstanceCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public string Etag { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }
        
        public DateTimeOffset? CancelledAt { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
            if (FinishedAt != null)
            {
                FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, FinishedAt.Value, nameof(FinishedAt), errorLocation);
                FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, FinishedAt.Value, nameof(FinishedAt), errorLocation);
            }
        }
    }

    /// <summary>
    /// Information about a specific version of a <see cref="WorkflowInstance"/>.
    /// </summary>
    public class WorkflowInstanceCreate : IValidatable
    {
        public string WorkflowVersionId { get; set; }

        public string Title { get; set; }

        public string InitialVersion { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InitialVersion, nameof(InitialVersion), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Title}";
    }
}