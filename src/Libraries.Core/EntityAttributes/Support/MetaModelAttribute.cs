using System;
using System.ComponentModel;
using System.Globalization;

namespace Nexus.Link.Libraries.Core.EntityAttributes.Support
{
    public abstract class MetaModelAttribute : Attribute
    {
        public string BusinessName { get; }
        public string Id { get; }

        protected MetaModelAttribute(string businessName, string id)
        {
            BusinessName = businessName;
            Id = id;
        }
    }
}