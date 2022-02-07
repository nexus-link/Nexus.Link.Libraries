using System;

namespace Nexus.Link.Libraries.Core.EntityAttributes.Support
{
    public abstract class OfficialAttribute : Attribute
    {
        public string Name { get; }
        public Guid Id { get; }

        protected OfficialAttribute(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
    }
}