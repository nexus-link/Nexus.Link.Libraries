using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    /// <summary>
    /// Inhertis from <see cref="TableItem"/> and adds time stamp columns.
    /// </summary>
    /// <remarks>Please note it is not mandatory to inherit from this class to use the functionality in this package. It is only provided as a convenience class.</remarks>
    public abstract class TimeStampedTableItem : TableItem, ITimeStamped
    {
        /// <inheritdoc />
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordUpdatedAt { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            var now = DateTimeOffset.UtcNow;
            FulcrumValidate.IsTrue(RecordCreatedAt <= now, errorLocation, $"Expected {nameof(RecordCreatedAt)} ({RecordCreatedAt.ToLogString()}) to have a value <= than the current time ({now.ToLogString()}).");
            FulcrumValidate.IsTrue(RecordUpdatedAt <= now, errorLocation, $"Expected {nameof(RecordUpdatedAt)} ({RecordUpdatedAt.ToLogString()}) to have a value <= than the current time ({now.ToLogString()}).");
        }
    }
}
