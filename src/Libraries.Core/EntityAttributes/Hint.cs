using System;
using System.Reflection;

namespace Nexus.Link.Libraries.Core.EntityAttributes
{
    public class Hint
    {
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
        public class SortOrderAttribute : Attribute
        {
            public int OrderNumber { get; }

            public SortOrderAttribute(int orderNumber)
            {
                OrderNumber = orderNumber;
            }
        }
        [AttributeUsage(AttributeTargets.Property)]
        public class OptimisticConcurrencyControlAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class PrimaryKeyAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class NullableAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class Crud : Attribute
        {
            public CrudTypeEnum CrudType { get; }

            public Crud(CrudTypeEnum crudType)
            {
                CrudType = crudType;
            }
        }

        public static PropertyInfo GetPrimaryKeyProperty<T>()
        {
            if (!typeof(T).IsClass) return null;
            return typeof(T).GetPropertyWithCustomAttribute<PrimaryKeyAttribute>();
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class RecordCreatedAtAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class RecordUpdatedAtAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class RequiredAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class UniqueAttribute : Attribute
        {
        }

        /// <summary>
        /// This means that the entity of this property has a relation to another entity.
        /// </summary>
        /// <remarks>
        /// If the <see cref="UniqueAttribute"/> is also set, then the
        /// <see cref="RequiredAttribute"/> must also be set.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Property)]
        public class ReferencesEntityAttribute : Attribute
        {
            public Type OtherEntity { get; }

            /// <summary>
            /// This means that the entity of this property has a relation to another entity.
            /// </summary>
            /// <remarks>
            /// Together with the attributes <see cref="RequiredAttribute"/> and <see cref="UniqueAttribute"/>,
            /// we can establish the multiplicity of the relation between
            /// <paramref name="otherEntity"/> and this entity.
            /// 1..1 (other) TO 0..many (this): Required, !Unique,
            /// 1..1 (other) TO 1..many (this): N/A
            /// 0..1 (other) TO 0..many (this): !Required, !Unique
            /// 0..1 (other) TO 1..many (this): N/A
            /// 1..1 (other) TO 0..1 (this): Required, Unique
            /// 1..1 (other) TO 1..1 (this): N/A
            /// 0..1 (other) TO 0..1 (this): N/A (!Required, Unique)
            /// many (other) TO many (this): N/A
            /// Required=false, Unique=false: zero to one TO zero to many
            /// Required=false, Unique=true: zero to one TO zero to one
            /// Required=true, Unique=false: one and only one TO zero to many
            /// Required=true, Unique=true: one and only one TO zero to one
            /// </remarks>
            public ReferencesEntityAttribute(Type otherEntity)
            {
                OtherEntity = otherEntity;
            }
        }
    }
}