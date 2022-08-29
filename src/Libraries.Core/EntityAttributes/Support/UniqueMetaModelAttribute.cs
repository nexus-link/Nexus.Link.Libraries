using System;

namespace Nexus.Link.Libraries.Core.EntityAttributes.Support;

public abstract class UniqueMetaModelAttribute : Attribute
{
    public string Id { get; set; }
    public string Name { get; set; }

}