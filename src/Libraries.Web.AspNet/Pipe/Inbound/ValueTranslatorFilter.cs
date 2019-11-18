
#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    public class ValueTranslatorFilter : IAsyncActionFilter
    {
        private readonly TranslatorSetup _translatorSetup;

        public ValueTranslatorFilter(TranslatorSetup translatorSetup)
        {
            InternalContract.RequireNotNull(translatorSetup, nameof(translatorSetup));
            InternalContract.RequireValidated(translatorSetup, nameof(translatorSetup));
            _translatorSetup = translatorSetup;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var translator = new Translator(_translatorSetup);
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var methodInfo = controllerActionDescriptor?.MethodInfo;
            if (methodInfo != null)
            {
                DecorateArguments(methodInfo.GetParameters(), context.ActionArguments, translator);
            }

            //Handle the request
            await next();

            if (methodInfo != null)
            {
                await DecorateResponseAsync(methodInfo.ReturnParameter, context, translator);
            }
        }

        private void DecorateArguments(ParameterInfo[] parameters, IDictionary<string, object> arguments, Translator translator)
        {
            foreach (var parameterInfo in parameters)
            {
                if (parameterInfo.ParameterType == typeof(string))
                {
                    DecorateValue(arguments, translator, parameterInfo);
                }
                else if (parameterInfo.ParameterType.IsClass)
                {
                    DecorateObject(arguments, translator, parameterInfo);
                }
            }
        }

        private async Task DecorateResponseAsync(ParameterInfo parameterInfo, ActionExecutingContext context, Translator translator)
        {
            if (context?.Result == null) return;
            if (!(context.Result is JsonResult jsonResult)) return;
            await translator.Add(jsonResult.Value).ExecuteAsync();
            jsonResult.Value = translator.Translate(jsonResult.Value);
        }

        private static void DecorateValue(IDictionary<string, object> arguments, Translator translator, ParameterInfo parameterInfo)
        {
            var parameterName = parameterInfo.Name;
            var attribute = parameterInfo.GetCustomAttribute<TranslationConceptAttribute>();
            var conceptName = attribute?.ConceptName;
            if (string.IsNullOrWhiteSpace(conceptName)) return;
            if (!arguments.ContainsKey(parameterName)) return;
            var currentValue = arguments[parameterName] as string;
            if (string.IsNullOrWhiteSpace(currentValue)) return;
            arguments[parameterName] = translator.Decorate(conceptName, currentValue);
        }

        private static void DecorateObject(IDictionary<string, object> arguments, Translator translator, ParameterInfo parameterInfo)
        {
            var parameterName = parameterInfo.Name;
            if (!arguments.ContainsKey(parameterName)) return;
            var currentValue = arguments[parameterName];
            if (currentValue == null) return;
            arguments[parameterName] = translator.DecorateItem(currentValue);
        }
    }
}
#endif
