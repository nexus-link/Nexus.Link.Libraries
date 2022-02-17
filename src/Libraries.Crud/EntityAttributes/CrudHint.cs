using System;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.Libraries.Crud.EntityAttributes
{
    public enum CrudMethodEnum
    {
        CreateAsync,
        ReadAsync,
        UpdateAsync
    }
    public static class CrudHint
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class OfficialMethodAttribute : Hint.OfficialMethodAttribute
        {
            public CrudMethodEnum CrudMethod { get; }

            public OfficialMethodAttribute(string name, CrudMethodEnum crudMethod, string id) : base(name, id)
            {
                CrudMethod = crudMethod;
            }
        }
    }
}