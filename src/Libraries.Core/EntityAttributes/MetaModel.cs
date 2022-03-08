using System;
using Nexus.Link.Libraries.Core.EntityAttributes.Support;

namespace Nexus.Link.Libraries.Core.EntityAttributes
{
    public static class MetaModel
    {
        [AttributeUsage(AttributeTargets.Interface)]
        public class CapabilityAttribute : MetaModelAttribute
        {
            public CapabilityAttribute(string businessName, string id) : base(businessName, id)
            {
            }
        }
        [AttributeUsage(AttributeTargets.Interface)]
        public class ServiceAttribute : MetaModelAttribute
        {
            public ServiceAttribute(string businessName, string id) : base(businessName, id)
            {
            }
        }
        [AttributeUsage(AttributeTargets.Method)]
        public class FunctionAttribute : MetaModelAttribute
        {
            public FunctionAttribute(string businessName, string id) : base(businessName, id)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CentralEntityAttribute : MetaModelAttribute
        {
            public CentralEntityAttribute(string businessName, string id) : base(businessName, id)
            {
            }
        }

        public class ChildToAttribute : Attribute
        {
            public Type EntityType { get; }
            public ParentCardinalityEnum ParentCardinality { get; } = ParentCardinalityEnum.OneAndOnlyOne;
            public ChildCardinalityEnum ChildCardinality { get; } = ChildCardinalityEnum.ZeroOrMore;

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
            ZeroOrMore,
            OneOrMore
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class CentralPropertyAttribute : MetaModelAttribute
        {
            public CentralPropertyAttribute(string businessName, string id) : base(businessName, id)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CapabilityEntityAttribute : Attribute
        {
            public Type CentralEntityType { get; }

            public CapabilityEntityAttribute(Type centralEntityType)
            {
                CentralEntityType = centralEntityType;
            }
        }

        public class CapabilityEntityReferralAttribute : Attribute
        {
            public Type CapabilityEntityType { get; }

            public CapabilityEntityReferralAttribute(Type capabilityEntityType)
            {
                CapabilityEntityType = capabilityEntityType;
            }
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class EntityParameterAttribute : CapabilityEntityReferralAttribute
        {
            public EntityParameterAttribute(Type capabilityEntityType)
                : base(capabilityEntityType)
            {
            }
        }

        [AttributeUsage(AttributeTargets.ReturnValue)]
        public class EntityReturnValueAttribute : CapabilityEntityReferralAttribute
        {
            public EntityReturnValueAttribute(Type capabilityEntityType)
                : base(capabilityEntityType)
            {
            }
        }

        public class CentralEntityPropertyReferralAttribute : Attribute
        {
            public Type CentralEntityType { get; }
            public string CentralPropertyName { get; }

            public CentralEntityPropertyReferralAttribute(Type centralEntityType, string centralPropertyName)
            {
                CentralEntityType = centralEntityType;
                CentralPropertyName = centralPropertyName;
            }
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class EntityPropertyParameterAttribute : CentralEntityPropertyReferralAttribute
        {
            public EntityPropertyParameterAttribute(Type centralEntityType, string centralPropertyName)
                : base(centralEntityType, centralPropertyName)
            {
            }
        }

        [AttributeUsage(AttributeTargets.ReturnValue)]
        public class EntityPropertyReturnValueAttribute : CentralEntityPropertyReferralAttribute
        {
            public EntityPropertyReturnValueAttribute(Type centralEntityType, string centralPropertyName)
                : base(centralEntityType, centralPropertyName)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class CapabilityPropertyAttribute : Attribute
        {
            public string CentralPropertyName { get; }

            public CapabilityPropertyAttribute(string centralPropertyName)
            {
                CentralPropertyName = centralPropertyName;
            }
        }
    }
}