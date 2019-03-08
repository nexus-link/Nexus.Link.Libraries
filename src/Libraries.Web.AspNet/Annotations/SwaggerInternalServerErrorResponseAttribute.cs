using System;
using System.Net;
using Nexus.Link.Libraries.Core.Error.Model;
#if NETCOREAPP
using Swashbuckle.AspNetCore.Annotations;
#else
using Swashbuckle.Swagger.Annotations;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Annotations
{
    /// <summary>
    /// Create the swagger documentation for a failure with <see cref="HttpStatusCode.InternalServerError"/> status code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerInternalServerErrorResponseAttribute : SwaggerResponseAttribute
    {
        /// <summary>
        /// Create the swagger documentation for a failure with <see cref="HttpStatusCode.InternalServerError"/> status code.
        /// </summary>
        public SwaggerInternalServerErrorResponseAttribute()
            : base((int)HttpStatusCode.InternalServerError,
                "The service had an internal error and could not fulfil the request completely. Please report this error and make sure that you attach any information that you find in the response body.",
                typeof(FulcrumError))
        {
        }
    }
}