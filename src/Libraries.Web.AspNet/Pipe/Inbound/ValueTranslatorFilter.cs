using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.Logging;
#if NETCOREAPP
using Nexus.Link.Libraries.Web.AspNet.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
#else
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Linq;

#endif

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
#if NETCOREAPP
    public class ValueTranslatorFilter : IAsyncActionFilter, IAsyncResultFilter
#else
    public class ValueTranslatorFilter : ActionFilterAttribute
#endif
    {
        /// <summary>
        /// The service that does the actual translation.
        /// </summary>
        public ITranslatorService TranslatorService { get; }

        private readonly Func<string> _getClientNameMethod;

        public ValueTranslatorFilter(ITranslatorService translatorService, Func<string> getClientNameMethod)
        {
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            TranslatorService = translatorService;
            _getClientNameMethod = getClientNameMethod;
        }

#if NETCOREAPP
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var translator = GetTranslator(context);
            if (translator != null)
            {
                var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var methodInfo = controllerActionDescriptor?.MethodInfo;

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
            }

            await next();
        }

        /// <inheritdoc />
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var translator = GetTranslator(context);
            if (translator != null)
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
#else
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var translator = GetTranslator(actionContext);
            if (translator != null)
            {
                var actionName = actionContext.ActionDescriptor.ActionName;
                var methodInfo = actionContext.ControllerContext.Controller.GetType()
                    .GetMethod(actionName, BindingFlags.Public | BindingFlags.Instance);

                if (methodInfo != null)
                {
                    try
                    {
                        DecorateArguments(methodInfo.GetParameters(), actionContext.ActionArguments, translator);
                    }
                    catch (Exception exception)
                    {
                        LogFailure(actionContext.Request, actionContext.Response, exception);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var translator = GetTranslator(actionExecutedContext);
            if (translator != null)
            {
                try
                {
                    await TranslateResponseAsync(actionExecutedContext, translator, cancellationToken);
                }
                catch (Exception exception)
                {
                    LogFailure(actionExecutedContext.Request, actionExecutedContext.Response, exception);
                    throw;
                }
            }
        }
#endif

        private ITranslator GetTranslator(object context)
        {
            if (context == null || TranslatorService == null) return null;
            var clientName = _getClientNameMethod();
            return clientName == null ? null : new Translator(clientName, TranslatorService);
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
            arguments[parameterName] = translator.Decorate(currentValue);
        }

#if NETCOREAPP

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
#else
        private static void LogFailure(HttpRequestMessage request, HttpResponseMessage response, Exception exception)
        {
            string requestAsLog;
            string resultAsLog;

            try
            {
                resultAsLog = response?.Content?.AsString();
            }
            catch (Exception e)
            {
                resultAsLog = $"Failed to serialize: {e.Message}";
            }

            try
            {
                requestAsLog = request?.ToLogString();
            }
            catch (Exception)
            {
                requestAsLog = request?.RequestUri.ToString();
            }

            Log.LogError($"Failed to decorate the arguments for the request {requestAsLog}. Result:\r{resultAsLog}", exception);
        }

        private static async Task TranslateResponseAsync(HttpActionExecutedContext context, ITranslator translator, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.ActionContext?.Response?.Content == null) return;
            var asString = context.ActionContext?.Response?.Content?.AsString();
            if (string.IsNullOrWhiteSpace(asString)) return;

            try
            {
                var objectResult = JObject.Parse(asString);
                var itemBeforeTranslation = objectResult;

                await translator.Add(itemBeforeTranslation).ExecuteAsync(cancellationToken);
                var itemAfterTranslation = translator.Translate(itemBeforeTranslation, itemBeforeTranslation.GetType());
                context.ActionContext.Response.Content = new StringContent(JsonConvert.SerializeObject(itemAfterTranslation), Encoding.UTF8, "application/json");

            }
            catch (JsonReaderException)
            {
                // This is ok; the content might not be JSON
            }
        }
#endif
    }
}
