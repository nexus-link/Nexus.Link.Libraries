using System;
#if NETCOREAPP
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Routing;
#else
using System.Web.Http.Routing;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Controllers
{
    public static class UrlLinkHelper
    {
        public static string LinkWithEnforcedHttps(this UrlHelper urlHelper, string routeName, object routeValues)
        {

#if NETCOREAPP

            var request = urlHelper.ActionContext.HttpContext.Request;
            if (!new Uri(request.GetDisplayUrl()).IsLoopback && !urlHelper.ActionContext.HttpContext.Request.IsHttps)
            {
                request.Scheme = Uri.UriSchemeHttps;
            }

#else

            // Inspired by https://stackoverflow.com/questions/24247402/generate-https-link-in-web-api-using-url-link#24248490

            if (!urlHelper.Request.RequestUri.IsLoopback && urlHelper.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                var secureUrlBuilder = new UriBuilder(urlHelper.Request.RequestUri) { Scheme = Uri.UriSchemeHttps };
                urlHelper.Request.RequestUri = new Uri(secureUrlBuilder.ToString());
            }

#endif

            return urlHelper.Link(routeName, routeValues);
        }
    }
}
