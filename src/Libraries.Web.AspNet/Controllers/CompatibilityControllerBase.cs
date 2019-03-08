#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Controllers
{
    /// <summary>
    /// Compatibility class. Will inherit from different classes; either Microsoft.AspNetCore.Mvc.ControllerBase
    /// for ASP.NET Core Web App or System.Web.Http.ApiController for ASP.NET Web Api.
    /// </summary>
    public abstract class CompatibilityControllerBase
#if NETCOREAPP
        : ControllerBase
#else
        : ApiController
#endif
    {
    }
}