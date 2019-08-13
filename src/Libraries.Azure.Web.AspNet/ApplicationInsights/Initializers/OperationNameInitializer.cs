#if NETCOREAPP
#else
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Nexus.Link.Libraries.Azure.Web.AspNet.ApplicationInsights.Initializers
{
    public class OperationNameInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is RequestTelemetry)
            {
                var context = HttpContext.Current;
                var operationName = "";
                //var parameterValues = "";
                if (context != null)
                {
                    var httpRequest = context.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
                    var httpVerb = context.Request.HttpMethod;

                    var routeData = httpRequest.GetRouteData();
                    var route = routeData.Route;
                    var tokens = route.DataTokens;

                    if (tokens != null && tokens.TryGetValue("actions", out var data))
                    {
                        var descriptors = (HttpActionDescriptor[])data;
                        if (descriptors != null)
                        {
                            var action = descriptors?.FirstOrDefault();
                            var controllerName = action?.ControllerDescriptor.ControllerName;
                            var actionName = action?.ActionName;
                            var paramCount = routeData.Values.Count;

                            var paramString = "[";
                            for (var i = 0; i < paramCount; i++)
                            {
                                var parameter = routeData.Values.ElementAt(i);
                                paramString += $"{parameter.Key}, ";

                            }
                            paramString = paramString.Remove(paramString.Length - 2) + "]";

                            operationName = string.IsNullOrEmpty(actionName)
                                ? $"{httpVerb} {route.RouteTemplate}"
                                : $"{httpVerb} {controllerName}/{actionName} {paramString}";
                        }
                    }
                    else
                    {
                        //Operation Name like route template
                        operationName = $"{httpVerb} {route.RouteTemplate} ";
                    }

                    //var token = route.DataTokens.FirstOrDefault(t => t.Key == "actions");

                    //if (token.Value != null)
                    //{
                    //    //Operation Name similar to .NET Core
                    //    var descriptors = token.Value as HttpActionDescriptor[];
                    //    var action = descriptors?.FirstOrDefault();

                    //    var actionName = action?.ActionName;
                    //    var paramCount = routeData.Values.Count;

                    //    var paramString = "[";

                    //    for (var i = 0; i < paramCount; i++)
                    //    {
                    //        var parameter = routeData.Values.ElementAt(i);
                    //        paramString += $"{parameter.Key}, ";
                    //        //parameterValues += $"[{parameter.Value}]";
                    //    }

                    //    paramString = paramString.Remove(paramString.Length - 2) + "]";
                    //    operationName = string.IsNullOrEmpty(actionName) ? $"{httpVerb} {route.RouteTemplate}" : $"{httpVerb} {actionName} {paramString}";
                    //}
                    //else
                    //{
                    //    //Operation Name like route template
                    //    operationName = $"{httpVerb} {route.RouteTemplate} ";
                    //}

                    if (!string.IsNullOrWhiteSpace(operationName))
                    {
                        telemetry.Context.Operation.Name = operationName;
                    }

                }
            }
        }
    }
}
#endif