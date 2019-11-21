﻿
#if NETCOREAPP
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Logging;
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
    public class ValueTranslatorFilter : IAsyncActionFilter, IAsyncResultFilter
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
                try
                {
                    DecorateArguments(methodInfo.GetParameters(), context.ActionArguments, translator);
                }
                catch (Exception exception)
                {
                    LogDecorationFailure(context, exception);
                }
            }

            await next();
        }

        /// <inheritdoc />
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
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
                try
                {
                    await TranslateResponseAsync(context, translator);
                }
                catch (Exception exception)
                {
                    await LogTranslationFailureAsync(context, exception);
                    throw;
                }
            }

            await next();
        }

        private static void DecorateArguments(IEnumerable<ParameterInfo> parameters,
            IDictionary<string, object> arguments, ITranslator translator)
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

        private static void DecorateValue(IDictionary<string, object> arguments, ITranslator translator,
            ParameterInfo parameterInfo)
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

        private static void DecorateObject(IDictionary<string, object> arguments, ITranslator translator,
            ParameterInfo parameterInfo)
        {
            var parameterName = parameterInfo.Name;
            if (!arguments.ContainsKey(parameterName)) return;
            var currentValue = arguments[parameterName];
            if (currentValue == null) return;
            arguments[parameterName] = translator.DecorateItem(currentValue);
        }

        private static void LogDecorationFailure(ActionExecutingContext context, Exception exception)
        {
            string requestAsLog;
            string resultAsLog;

            try
            {
                resultAsLog = JsonConvert.SerializeObject(context.Result, Formatting.Indented);
            }
            catch (Exception e)
            {
                resultAsLog = $"Failed to serialize: {e.Message}";
            }

            try
            {
                requestAsLog = context.HttpContext.Request.ToLogString();
            }
            catch (Exception)
            {
                requestAsLog = context.HttpContext?.Request?.Path;
            }

            Log.LogError(
                $"Failed to decorate the arguments for the request {requestAsLog}. Result:\r{resultAsLog}",
                exception);
        }

        private static async Task TranslateResponseAsync(ResultExecutingContext context, ITranslator translator,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context?.Result == null) return;
            if (!(context.Result is ObjectResult objectResult)) return;

            var itemBeforeTranslation = objectResult.Value;

            await translator.Add(itemBeforeTranslation).ExecuteAsync(cancellationToken);
            var itemAfterTranslation = translator.Translate(itemBeforeTranslation,itemBeforeTranslation.GetType());
            context.Result = new ObjectResult(itemAfterTranslation);
        }

        private static async Task LogTranslationFailureAsync(ResultExecutingContext context, Exception exception)
        {
            string requestAsLog;
            string resultAsLog;

            try
            {
                resultAsLog = JsonConvert.SerializeObject(context.Result, Formatting.Indented);
            }
            catch (Exception e)
            {
                resultAsLog = $"Failed to serialize: {e.Message}";
            }

            try
            {
                requestAsLog = await context.HttpContext.Request.ToLogStringAsync(context.HttpContext.Response);
            }
            catch (Exception)
            {
                requestAsLog = context.HttpContext?.Request?.Path;
            }

            Log.LogError(
                $"Failed to translate the response for the request {requestAsLog}. Result:\r{resultAsLog}",
                exception);
        }
    }
}
#endif
