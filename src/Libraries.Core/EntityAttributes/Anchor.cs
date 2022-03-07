using System;
using Nexus.Link.Libraries.Core.EntityAttributes.Support;

namespace Nexus.Link.Libraries.Core.EntityAttributes
{
    public static class Anchor
    {
        [AttributeUsage(AttributeTargets.Interface)]
        public class CapabilityAttribute : OfficialAttribute
        {
            public CapabilityAttribute(string name, string id) : base(name, id)
            {
            }
        }
        [AttributeUsage(AttributeTargets.Interface)]
        public class ServiceAttribute : OfficialAttribute
        {
            public ServiceAttribute(string name, string id) : base(name, id)
            {
            }
        }
        [AttributeUsage(AttributeTargets.Method)]
        public class MethodAttribute : OfficialAttribute
        {
            public MethodAttribute(string name, string id) : base(name, id)
            {
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        public class RootEntityAttribute : OfficialAttribute
        {
            public RootEntityAttribute(string name, string id) : base(name, id)
            {
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        public class CapabilityEntityAttribute : Attribute
        {
            public Type RootEntityType { get; }

            public CapabilityEntityAttribute(Type rootEntityType)
            {
                RootEntityType = rootEntityType;
            }
        }

        public class ChildToAttribute : Attribute
        {
            public Type EntityType { get; }
            public ParentCardinalityEnum ParentCardinality { get; } = ParentCardinalityEnum.OneAndOnlyOne;
            public ChildCardinalityEnum ChildCardinality { get; } = ChildCardinalityEnum.ZeroOrOneOrMany;

            public ChildToAttribute(Type entityType)
            {
                EntityType = entityType;
            }
        }

        public enum ParentCardinalityEnum
        {
            ZeroOrOne,
            OneAndOnlyOne
        }



        public enum ChildCardinalityEnum
        {
            ZeroOrOneOrMany,
            OneOrMany
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class PropertyAttribute : OfficialAttribute
        {
            public PropertyAttribute(string name, string id) : base(name, id)
            {
            }
        }
    }
}