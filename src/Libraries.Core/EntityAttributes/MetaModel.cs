using System;
using Nexus.Link.Libraries.Core.EntityAttributes.Support;

namespace Nexus.Link.Libraries.Core.EntityAttributes;

public static class MetaModel
{
    /// <summary>
    /// Capability
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class CapabilityAttribute : UniqueMetaModelAttribute
    {
    }

    /// <summary>
    /// Service
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceAttribute : UniqueMetaModelAttribute
    {
        [Validation.NotNull]
        public Type EntityType { get; }
        public Type ParentEntity { get; set; }

        public ServiceAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionAttribute : UniqueMetaModelAttribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FunctionParameterAttribute : UniqueMetaModelAttribute
    {
    }
    [AttributeUsage(AttributeTargets.ReturnValue)]
    public class FunctionReturnValueAttribute : UniqueMetaModelAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : UniqueMetaModelAttribute
    {
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

    public class EntityReferenceAttribute : Attribute
    {
        public Type EntityType { get; }

        public EntityReferenceAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }

    public class CentralEntityAttributeReferralAttribute : Attribute
    {
        public Type CentralEntityType { get; }
        public string CentralAttributeName { get; }

        public CentralEntityAttributeReferralAttribute(Type centralEntityType, string centralAttributeName)
        {
            CentralEntityType = centralEntityType;
            CentralAttributeName = centralAttributeName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CentralEntityAttribute : Attribute
    {
        public Type CentralEntityType { get; }

        public CentralEntityAttribute(Type centralEntityId)
        {
            CentralEntityType = centralEntityId;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AttributeAttribute : UniqueMetaModelAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CentralAttributeAttribute : Attribute
    {
        public string CentralAttributeName { get; }

        public CentralAttributeAttribute(string centralAttributeName)
        {
            CentralAttributeName = centralAttributeName;
        }
    }
}