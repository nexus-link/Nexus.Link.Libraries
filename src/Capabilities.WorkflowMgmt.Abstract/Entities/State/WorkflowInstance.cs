using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State
{
    public enum WorkflowStateEnum 
    {
        /// <summary>
        /// The activity has been started (default value)
        /// </summary>
        /// <remarks>
        /// The default value for this enumeration
        /// </remarks>
        Executing,
        /// <summary>
        /// We are asynchronously waiting for the activity to finish
        /// </summary>
        Waiting,
        /// <summary>
        /// There is at least one activity that that has a problem, but there are some parts of the workflow that still are running.
        /// </summary>
        Halting,
        /// <summary>
        /// There is at least one activity that that has a problem and no activities can run until the problem has been resolved.
        /// </summary>
        Halted,
        /// <summary>
        /// The activity has finished successfully
        /// </summary>
        Success,
        /// <summary>
        /// The activity has finished, but it failed. <see cref="ActivityFailUrgencyEnum"/> for the level of urgency to deal with this.
        /// </summary>
        Failed
    };
    /// <summary>
    /// Information about an instance of a <see cref="WorkflowVersion"/>.
    /// </summary>
    public class WorkflowInstance : WorkflowInstanceCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public string Etag { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
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

        public WorkflowStateEnum State { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }
        
        public DateTimeOffset? CancelledAt { get; set; }

        public bool IsComplete { get; set; }

        public string ResultAsJson { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowVersionId, nameof(WorkflowVersionId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InitialVersion, nameof(InitialVersion), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
            if (FinishedAt != null)
            {
                FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, FinishedAt.Value, nameof(FinishedAt), errorLocation);
                FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, FinishedAt.Value, nameof(FinishedAt), errorLocation);
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"{Title}";
    }
}