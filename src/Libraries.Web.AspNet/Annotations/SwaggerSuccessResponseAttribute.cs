using System;
using System.Net;
#if NETCOREAPP
using Swashbuckle.AspNetCore.Annotations;
#else
using Swashbuckle.Swagger.Annotations;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Annotations
{
    /// <summary>
    /// Create the swagger documentation for a success with a <see cref="HttpStatusCode.OK"/>
    /// or <see cref="HttpStatusCode.NoContent"/> status code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerSuccessResponseAttribute : SwaggerResponseAttribute
    {
        /// <summary>
        /// Create the swagger documentation for a success with a <see cref="HttpStatusCode.OK"/>
        /// or <see cref="HttpStatusCode.NoContent"/> status code.
        /// </summary>
        /// <param name="type">The type for the returned result. Null means that the code is
        /// <see cref="HttpStatusCode.NoContent"/>, otherwise the code is <see cref="HttpStatusCode.OK"/>.</param>
        public SwaggerSuccessResponseAttribute(Type type = null)
            : base(
                type == null ? (int)HttpStatusCode.NoContent : (int)HttpStatusCode.OK,
                type == null ? "Success. No response body." : "Success. The response body contains the result.",
                type)
        {
        }
    }
}