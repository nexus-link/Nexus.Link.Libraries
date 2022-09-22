using System;

namespace Nexus.Link.Libraries.Crud.EntityAttributes
{
    /// <inheritdoc />
    public class Hint : Core.EntityAttributes.Hint
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class Crud : Attribute
        {
            public CrudTypeEnum CrudType { get; }

            public Crud(CrudTypeEnum crudType)
            {
                CrudType = crudType;
            }
        }
    }
}