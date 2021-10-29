using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration
{
    public class WorkflowForm : WorkflowFormCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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
    /// Information about a workflow.
    /// </summary>
    /// <remarks>
    /// <see cref="CapabilityName"/> and <see cref="Title"/> in combination must be unique.
    /// </remarks>
    public class WorkflowFormCreate : IValidatable
    {
        public string CapabilityName {get; set; }
        /// <summary>
        /// The name of the work flow
        /// </summary>
        public string Title {get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(CapabilityName, nameof(CapabilityName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Title}";
    }
}