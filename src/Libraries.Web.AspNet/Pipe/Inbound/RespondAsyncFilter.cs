﻿#if NETCOREAPP
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Web.AspNet.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync;
using Nexus.Link.Libraries.Web.AspNet.Queue;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    public class RespondAsyncFilter : IAsyncActionFilter
    {
        private readonly IRespondAsyncFilterSupport _respondAsyncFilterSupport;

        public RespondAsyncFilter(IRespondAsyncFilterSupport respondAsyncFilterSupport)
        {
            _respondAsyncFilterSupport = respondAsyncFilterSupport;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            if (_respondAsyncFilterSupport.IsRunningAsynchronously(request))
            {
                FulcrumApplication.Context.ExecutionIsAsynchronous = true;
            }
            else
            {
                var cancellationToken = context.HttpContext.RequestAborted;
                var clientPrefersAsync = CheckIfClientPrefersAsynchronousExecution(context.HttpContext.Request);
                var methodOpinion = GetMethodOpinion(context.ActionDescriptor);
                bool respondAsync;

                switch (methodOpinion)
                {
                    case RespondAsyncOpinionEnum.Indifferent:
                        respondAsync = clientPrefersAsync;
                        break;
                    case RespondAsyncOpinionEnum.Never:
                        respondAsync = false;
                        break;
                    case RespondAsyncOpinionEnum.Always:
                        respondAsync = true;
                        break;
                    default:
                        FulcrumAssert.Fail(CodeLocation.AsString(),
                            $"Unknown value for {nameof(RespondAsyncOpinionEnum)}: {methodOpinion}");
                        throw new ArgumentOutOfRangeException();
                }

                if (respondAsync)
                {
                    // This request should be responded to asynchronously, so put it on a queue and respond now with a
                    // 202 Accepted response that contains information on how to get the response when it is available.
                    try
                    {
                        var requestId =
                            await _respondAsyncFilterSupport.EnqueueAsync(context.HttpContext.Request, cancellationToken);
                        context.Result = await _respondAsyncFilterSupport.GetActionResultAsync(requestId, cancellationToken);
                        return;
                    }
                    catch (QueueFullException e)
                    {
                        throw new FulcrumTryAgainException(
                            $"The queue for asynchronous request execution is full: {e.Message}");
                    }
                }
            }

            await next();
        }

        private static bool CheckIfClientPrefersAsynchronousExecution(HttpRequest request)
        {
            return request.Headers.TryGetValue(Constants.PreferHeaderName, out var preferHeader)
                   && ((ICollection<string>)preferHeader).Contains(Constants.PreferRespondAsyncHeaderValue);
        }

        private static RespondAsyncOpinionEnum GetMethodOpinion(ActionDescriptor actionDescriptor)
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            var methodInfo = controllerActionDescriptor?.MethodInfo;
            var preferAsyncAttribute = methodInfo?.GetCustomAttribute<RespondAsyncAttribute>();
            return preferAsyncAttribute?.Opinion ?? RespondAsyncOpinionEnum.Indifferent;
        }
    }

    /// <summary>
    /// https://stackoverflow.com/questions/55990151/is-adding-addmvc-service-twice-in-configureservices-a-good-practice-in-asp-n
    /// </summary>
    public class RespondAsyncFilterConfigureOptions : IConfigureOptions<MvcOptions>
    {
        private readonly IRespondAsyncFilterSupport _responseFilterSupport;

        public RespondAsyncFilterConfigureOptions(IRespondAsyncFilterSupport responseFilterSupport)
        {
            _responseFilterSupport = responseFilterSupport;
        }
        /// <inheritdoc />
        public void Configure(MvcOptions options)
        {
            var filter = new RespondAsyncFilter(_responseFilterSupport);
            options.Filters.Add(filter);
        }
    }

    /// <summary>
    /// Convenience method for adding the <see cref="ValueTranslatorFilter"/>.
    /// </summary>
    public static class RespondAsyncFilterExtensions
    {
        /// <summary>
        /// Add the <see cref="RespondAsyncFilter"/> to MVC.
        /// </summary>
        public static IServiceCollection AddRespondAsyncFilter(this IServiceCollection services)
        {
            services.ConfigureOptions<ValueTranslatorFilterConfigureOptions>();
            return services;
        }
    }
}
#endif