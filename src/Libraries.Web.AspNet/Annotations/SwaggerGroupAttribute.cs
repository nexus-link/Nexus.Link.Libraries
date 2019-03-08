using System;
#if NETCOREAPP
using Swashbuckle.AspNetCore.Annotations;
#else
using Swashbuckle.Swagger.Annotations;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Annotations
{
    /// <summary>
    /// Set the Swagger group for the method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerGroupAttribute : SwaggerOperationAttribute
    {
        /// <summary>
        /// Set the Swagger group for the method.
        /// </summary>
        /// <param name="groupName">The name of the Swagger group for this method.</param>
        public SwaggerGroupAttribute(string groupName)
            : base()
        {
            Tags = new[] { groupName };
        }
    }
}
