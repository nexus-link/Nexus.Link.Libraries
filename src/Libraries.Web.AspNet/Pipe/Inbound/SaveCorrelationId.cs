using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Pipe;

#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Web.AspNet.Logging;
#else
using System.Linq;
using Nexus.Link.Libraries.Web.Logging;
#endif
namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// Handle correlation id on inbound requests.
    /// </summary>
    public class SaveCorrelationId : CompatibilityDelegatingHandlerWithCancellationSupport
    {
        private static readonly DelegateState DelegateState = new DelegateState(typeof(SaveCorrelationId).FullName);

        /// <summary>
        /// True if this delegate has started in the current context
        /// </summary>
        public static bool HasStarted
        {
            get => DelegateState.HasStarted;
            private set => DelegateState.HasStarted = value;
        }

#if NETCOREAPP
        /// <inheritdoc />
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public SaveCorrelationId(RequestDelegate next)
            : base(next)
        {
        }
#else
        public SaveCorrelationId()
        {
        }
#endif
        /// <inheritdoc />
        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(SaveCorrelationId)} has already been started in this http request.");
            InternalContract.Require(!BatchLogs.HasStarted,
                $"{nameof(BatchLogs)} must not precede {nameof(SaveCorrelationId)}");
            InternalContract.Require(!LogRequestAndResponse.HasStarted,
                $"{nameof(LogRequestAndResponse)} must not precede {nameof(SaveCorrelationId)}");
            InternalContract.Require(!ExceptionToFulcrumResponse.HasStarted,
                $"{nameof(ExceptionToFulcrumResponse)} must not precede {nameof(SaveCorrelationId)}");
            HasStarted = true;
            SaveCorrelationIdToExecutionContext(context);
            await CallNextDelegateAsync(context, cancellationToken);
        }

        /// <summary>
        /// Read the correlation id header from the <paramref name="context"/> request and save it to the execution context.
        /// </summary>
        /// <param name="context">The request that we will read the header from.</param>
        /// <returns></returns>
        /// <remarks>This method is made public for testing purposes. Should normally not be called from outside this class.</remarks>
        public void SaveCorrelationIdToExecutionContext(CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
#else
            var request = context?.RequestMessage;
#endif
            var correlationId = ExtractCorrelationIdFromHeader(context);
            var createCorrelationId = string.IsNullOrWhiteSpace(correlationId);
            if (createCorrelationId) correlationId = Guid.NewGuid().ToString();
            FulcrumApplication.Context.CorrelationId = correlationId;
            if (createCorrelationId)
            {
                Log.LogInformation(
                    $"Created correlation id {correlationId}, as incoming request did not have it. ({request?.ToLogString()})");
            }
        }

        internal static string ExtractCorrelationIdFromHeader(CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
#else
            var request = context?.RequestMessage;
#endif

            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
#if NETCOREAPP
            var correlationHeaderValueExists =
                request.Headers.TryGetValue(Constants.FulcrumCorrelationIdHeaderName, out var correlationIds);
#else
            var correlationHeaderValueExists =
                request.Headers.TryGetValues(Constants.FulcrumCorrelationIdHeaderName, out var correlationIds);
#endif
            if (!correlationHeaderValueExists) return null;
            var correlationsArray = correlationIds.ToArray();
            if (correlationsArray.Length > 1)
            {
                // ReSharper disable once UnusedVariable
                var message =
                    $"There was more than one correlation id in the header: {string.Join(", ", correlationsArray)}. The first one was picked as the Fulcrum correlation id from here on.";
                Log.LogWarning(message);
            }

            return correlationsArray[0];
        }
    }
#if NETCOREAPP
    public static class SaveCorrelationIdExtension
    {
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public static IApplicationBuilder UseNexusSaveCorrelationId(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SaveCorrelationId>();
        }
    }
#endif
}