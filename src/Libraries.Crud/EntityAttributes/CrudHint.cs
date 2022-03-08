﻿using System;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.EntityAttributes.Support;

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
        public class CentralMethodAttribute : MetaModelAttribute
        {
            public CrudMethodEnum CrudMethod { get; }

            public CentralMethodAttribute(string name, CrudMethodEnum crudMethod, string id) : base(name, id)
            {
                CrudMethod = crudMethod;
            }
        }
    }
}