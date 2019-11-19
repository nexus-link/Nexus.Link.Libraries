
#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    public class ValueTranslatorFilter : IAsyncActionFilter
    {
        public TranslatorFactory TranslatorFactory { get; set; }

        public ValueTranslatorFilter()
        {
        }

        public ValueTranslatorFilter(TranslatorFactory translatorFactory)
        {
            InternalContract.RequireNotNull(translatorFactory, nameof(translatorFactory));
            InternalContract.RequireValidated(translatorFactory, nameof(translatorFactory));
            TranslatorFactory = translatorFactory;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ITranslator translator = null;
            MethodInfo methodInfo = null;
            if (FulcrumApplication.IsInDevelopment)
            {
                InternalContract.Require(TranslatorFactory != null, 
                    $"You must set the {nameof(TranslatorFactory)} property of {nameof(ValueTranslatorFilter)}.");
            }
            if (TranslatorFactory != null)
            {
                translator = TranslatorFactory.CreateTranslator();
                var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                methodInfo = controllerActionDescriptor?.MethodInfo;
            }
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

        private void DecorateArguments(ParameterInfo[] parameters, IDictionary<string, object> arguments, ITranslator translator)
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

        private async Task DecorateResponseAsync(ParameterInfo parameterInfo, ActionExecutingContext context, ITranslator translator, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context?.Result == null) return;
            if (!(context.Result is JsonResult jsonResult)) return;
            await translator.Add(jsonResult.Value).ExecuteAsync(cancellationToken);
            jsonResult.Value = translator.Translate(jsonResult.Value);
        }

        private static void DecorateValue(IDictionary<string, object> arguments, ITranslator translator, ParameterInfo parameterInfo)
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

        private static void DecorateObject(IDictionary<string, object> arguments, ITranslator translator, ParameterInfo parameterInfo)
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
