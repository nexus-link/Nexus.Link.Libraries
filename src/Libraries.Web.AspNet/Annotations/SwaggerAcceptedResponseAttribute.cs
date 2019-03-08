using System;
using System.Net;
using Nexus.Link.Libraries.Core.Misc.Models;
#if NETCOREAPP
using Swashbuckle.AspNetCore.Annotations;
#else
using Swashbuckle.Swagger.Annotations;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Annotations
{
    /// <summary>
    /// Create the swagger documentation for a success with a <see cref="HttpStatusCode.Accepted"/> status code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerAcceptedResponseAttribute : SwaggerResponseAttribute
    {
        /// <summary>
        /// Create the swagger documentation for a success with a <see cref="HttpStatusCode.Accepted"/> status code.
        /// </summary>
        public SwaggerAcceptedResponseAttribute()
            : base((int)HttpStatusCode.Accepted, "The request has been accepted. The response body contains information about how to follow up on the progress of the task.", typeof(TryAgainLater))
        {
        }
    }
}