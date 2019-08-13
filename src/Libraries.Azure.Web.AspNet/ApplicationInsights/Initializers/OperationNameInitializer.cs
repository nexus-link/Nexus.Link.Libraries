#if NETCOREAPP
#else
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        /// ITelemetryInitializer that sets the operation name of request telemetry without parameter values
        /// </summary>
        /// <param name="filteredParameters">List of parameter names that will be removed if present</param>
        public OperationNameInitializer(List<string> filteredParameters = null)
        {
            if (filteredParameters != null && filteredParameters.Count > 0)
            {
                FilteredParameterNames.AddRange(filteredParameters);
            }
        }
        public List<string> FilteredParameterNames = new List<string>();

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is RequestTelemetry)
            {
                var context = HttpContext.Current;
                var operationName = "";
                var paramString = "";
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
                            var parameters = action?.GetParameters();

                            if (parameters != null && parameters.Count > 0)
                            {
                                var parameterList = parameters.Where(p => !FilteredParameterNames.Contains(p.ParameterName)).Select(p => p.ParameterName).ToList();
                                if (parameterList.Count > 0)
                                {
                                    paramString = "[" + string.Join(",", parameterList) + "]";
                                }

                            }

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