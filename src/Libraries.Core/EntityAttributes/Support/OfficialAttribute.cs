using System;

namespace Nexus.Link.Libraries.Core.EntityAttributes.Support
{
    public abstract class OfficialAttribute : Attribute
    {
        public string Name { get; }
        public string Id { get; }

        protected OfficialAttribute(string name, string id)
        {
            Name = name;
            Id = id;
        }
    }
}