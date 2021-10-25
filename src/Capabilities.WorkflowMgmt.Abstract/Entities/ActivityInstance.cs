using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    public enum ActivityExceptionCategoryEnum 
    {
        /// <summary>
        /// The activity had a maximum time to execute and it was exceeded.
        /// </summary>
        Timeout,
        /// <summary>
        /// None of the current categories was fitting for this exception.
        /// </summary>
        Other
    };
    public enum ActivityStateEnum 
    {
        /// <summary>
        /// The activity has been started
        /// </summary>
        Started,
        /// <summary>
        /// We are asynchronously waiting for the activity to finish
        /// </summary>
        Waiting,
        /// <summary>
        /// The activity has finished successfully
        /// </summary>
        Success,
        /// <summary>
        /// The activity has finished, but it failed. <see cref="ActivityFailUrgencyEnum"/> for the level of urgency to deal with this.
        /// </summary>
        Failed
    };
    public enum ActivityFailUrgencyEnum 
    {
        /// <summary>
        /// If this activity fails, the entire workflow should be cancelled.
        /// </summary>
        CancelWorkflow,
        /// <summary>
        /// The activity is hindering other activities to complete.
        /// </summary>
        Stopping,
        /// <summary>
        /// The activity does not hinder other activities, in fact the whole workflow can deliver a result even if this activity fails.
        /// It is important that the activity is completed anyway, so it should be dealt with eventually to complete the workflow entirely.
        /// </summary>
        HandleLater,
        /// <summary>
        /// The activity was of a "fire and forget" character, i.e. it doesn't matter if it failed, as long as we tried.
        /// </summary>
        Ignore
    };
    public class ActivityInstance : ActivityInstanceCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
    {
        public string Id { get; set; }
        public string Etag { get; set; }

        public DateTimeOffset StartedAt{ get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public string AsyncRequestId { get; set; }

        public string ResultAsJson { get; set; }

        public bool? ExceptionAlertHandled { get; set; }

        public ActivityExceptionCategoryEnum? ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
            if (State == ActivityStateEnum.Failed)
            {
                FulcrumValidate.IsNotNull(ExceptionCategory, nameof(ExceptionCategory), errorLocation);
                FulcrumValidate.IsNotNullOrWhiteSpace(ExceptionTechnicalMessage, nameof(ExceptionTechnicalMessage), errorLocation);
                FulcrumValidate.IsNotNullOrWhiteSpace(ExceptionFriendlyMessage, nameof(ExceptionFriendlyMessage), errorLocation);
            }
            

            if (ExceptionCategory != null)
            {
                FulcrumValidate.AreEqual(ActivityStateEnum.Failed, State, nameof(State), errorLocation,
                    $"Inconsistency: {nameof(State)} can't have value {State} if {nameof(ExceptionCategory)} is not null.");
            }

            if (ExceptionTechnicalMessage != null)
            {
                FulcrumValidate.AreEqual(ActivityStateEnum.Failed, State, nameof(State), errorLocation,
                    $"Inconsistency: {nameof(State)} can't have value {State} if {nameof(ExceptionTechnicalMessage)} is not null.");
            }

            if (ExceptionFriendlyMessage != null)
            {
                FulcrumValidate.AreEqual(ActivityStateEnum.Failed, State, nameof(State), errorLocation,
                    $"Inconsistency: {nameof(State)} can't have value {State} if {nameof(ExceptionFriendlyMessage)} is not null.");
            }
        }
    }

    public class ActivityInstanceCreate : ActivityInstanceUnique, IValidatable
    {
        public ActivityStateEnum State { get; set; }

        public ActivityFailUrgencyEnum FailUrgency { get; set; }

        public bool HasCompleted => State == ActivityStateEnum.Success || State == ActivityStateEnum.Failed;

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(ActivityVersionId, nameof(ActivityVersionId), errorLocation);
            if (ParentIteration.HasValue) FulcrumValidate.IsGreaterThanOrEqualTo(1, ParentIteration.Value, nameof(ParentIteration), errorLocation);
        }
    }

    public class ActivityInstanceUnique
    {
        public string WorkflowInstanceId { get; set; }
        public string ActivityVersionId { get; set; }
        public string ParentActivityInstanceId { get; set; }
        public int? ParentIteration { get; set; }
    }
}