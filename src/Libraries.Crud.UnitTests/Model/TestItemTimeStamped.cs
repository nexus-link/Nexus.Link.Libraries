﻿using System;
using Nexus.Link.Libraries.Core.Storage.Model;

#pragma warning disable 659

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A uniquely identifiable item that implements <see cref="ITimeStamped"/> to be used in testing
    /// </summary>
    public partial class TestItemTimestamped<TId> : TestItemId<TId>, ITimeStamped
    {
        /// <inheritdoc />
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordUpdatedAt { get; set; }
    }

    #region override object
    public partial class TestItemTimestamped<TId>
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is TestItemTimestamped<TId> o)) return false;
            if (!base.Equals(obj)) return false;
            return RecordCreatedAt.Equals(o.RecordCreatedAt) && RecordUpdatedAt.Equals(o.RecordUpdatedAt);
        }
    }
    #endregion
}
