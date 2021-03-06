﻿using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Test.Cache
{
    public class ItemWithParentId : IUniquelyIdentifiable<Guid>
    {
        public ItemWithParentId()
        {
        }

        public ItemWithParentId(Guid id, string value, Guid? parentId = null)
        {
            Id = id;
            Value = value;
            ParentId = parentId;
        }

        /// <inheritdoc />
        public Guid Id { get; set; }

        public string Value { get; set; }

        public Guid? ParentId { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is ItemWithParentId item)) return false;
            return Equals(item);
        }

        protected bool Equals(ItemWithParentId other)
        {
            return string.Equals(Value, other.Value) && ParentId.Equals(other.ParentId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            var hashCode = Value.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }
    }
}