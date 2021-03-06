﻿using System;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
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
            if (!IsLoopback(new Uri(request.GetDisplayUrl())) && !urlHelper.ActionContext.HttpContext.Request.IsHttps)
            {
                request.Scheme = Uri.UriSchemeHttps;
                if (request.Host.Port.HasValue && request.Host.Port == 80) request.Host = new HostString(request.Host.Host, 443);
            }

#else

            // Inspired by https://stackoverflow.com/questions/24247402/generate-https-link-in-web-api-using-url-link#24248490

            if (!IsLoopback(urlHelper.Request.RequestUri) && urlHelper.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                var secureUrlBuilder = new UriBuilder(urlHelper.Request.RequestUri)
                {
                    Scheme = Uri.UriSchemeHttps
                };
                if (secureUrlBuilder.Port == 80) secureUrlBuilder.Port = 443;
                urlHelper.Request.RequestUri = new Uri(secureUrlBuilder.ToString());
            }

#endif

            return urlHelper.Link(routeName, routeValues);
        }

        private static bool IsLoopback(this Uri requestUri)
        {
            if (requestUri.IsLoopback) return true;
            if (requestUri.Host == "nexus.local") return true;
            if (requestUri.Host.EndsWith("nexus.local")) return true;
            return false;
        }
    }
}
