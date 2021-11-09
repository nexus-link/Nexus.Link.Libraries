using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State
{
    public enum ActivityExceptionCategoryEnum 
    {
        /// <summary>
        /// The exception for this activity was of category "technical problems" and should probably be inspected by a technician.
        /// </summary>
        /// <remarks>
        /// The default value for this enumeration
        /// </remarks>
        TechnicalError,
        /// <summary>
        /// The exception for this activity was due to an error in the Workflow mgmt capability. Please inform the implementor of the capability.
        /// </summary>
        WorkflowCapabilityError,
        /// <summary>
        /// The exception for this activity was due to an error in implementation of the workflow, i.e. a programmers error.
        /// </summary>
        WorkflowImplementationError,
        /// <summary>
        /// The exception for this activity was of category "business problem" and should probably be inspected by a business person.
        /// </summary>
        BusinessError
    };
    public enum ActivityStateEnum 
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
        /// The activity has finished successfully
        /// </summary>
        Success,
        /// <summary>
        /// The activity has finished, but it failed. <see cref="ActivityFailUrgencyEnum"/> for the level of urgency to deal with this.
        /// </summary>
        Failed
    };
    public class ActivityInstance : ActivityInstanceCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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

    public class ActivityInstanceCreate : ActivityInstanceUnique, IValidatable
    {
        public ActivityStateEnum State { get; set; }

        public DateTimeOffset StartedAt{ get; set; }

        public bool HasCompleted => State == ActivityStateEnum.Success || State == ActivityStateEnum.Failed;

        public DateTimeOffset? FinishedAt { get; set; }

        public string AsyncRequestId { get; set; }

        public string ResultAsJson { get; set; }

        public bool? ExceptionAlertHandled { get; set; }

        public ActivityExceptionCategoryEnum? ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(ActivityVersionId, nameof(ActivityVersionId), errorLocation);
            if (ParentIteration.HasValue) FulcrumValidate.IsGreaterThanOrEqualTo(1, ParentIteration.Value, nameof(ParentIteration), errorLocation);

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

        /// <inheritdoc />
        public override string ToString() => $"{State}";
    }

    public class ActivityInstanceUnique
    {
        public string WorkflowInstanceId { get; set; }
        public string ActivityVersionId { get; set; }
        public string ParentActivityInstanceId { get; set; }
        public int? ParentIteration { get; set; }
    }
}