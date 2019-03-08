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
    /// Create the swagger documentation for a failure with <see cref="HttpStatusCode.BadRequest"/> status code.
    /// </summary>
     [AttributeUsage(AttributeTargets.Method)]
       public class SwaggerBadRequestResponseAttribute : SwaggerResponseAttribute
       {
           /// <summary>
           /// Create the swagger documentation for a failure with <see cref="HttpStatusCode.BadRequest"/> status code.
           /// </summary>
           public SwaggerBadRequestResponseAttribute()
               : base((int)HttpStatusCode.BadRequest,
                   "Bad request. The service could not accept the request. See the body for more information.", 
                   typeof(FulcrumError))
           {
           }
       }
}
