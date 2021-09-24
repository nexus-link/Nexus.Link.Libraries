﻿using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    public class ActivityForm : ActivityFormCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
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

    public class ActivityFormCreate : IValidatable
    {
        public string WorkflowFormId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Title, nameof(Title), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Type, nameof(Type), errorLocation);
        }
    }
}