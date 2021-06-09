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
#else
using System.Linq;
#endif
namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// Handle <see cref="Constants.NexusTestContextHeaderName"/> header on inbound requests.
    /// </summary>
    public class SaveNexusTestContext : CompatibilityDelegatingHandlerWithCancellationSupport
    {
        private static readonly DelegateState DelegateState = new DelegateState(typeof(SaveNexusTestContext).FullName);

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
        public SaveNexusTestContext(RequestDelegate next)
            : base(next)
        {
        }
#else
        public SaveNexusTestContext()
        {
        }
#endif
        /// <inheritdoc />
        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(SaveNexusTestContext)} has already been started in this http request.");
            HasStarted = true;
            SaveHeaderToExecutionContext(context);
            await CallNextDelegateAsync(context, cancellationToken);
        }

        /// <summary>
        /// Read the <see cref="Constants.NexusTestContextHeaderName"/> header from the <paramref name="context"/> request and save it to the execution context.
        /// </summary>
        /// <param name="context">The request that we will read the header from.</param>
        /// <returns></returns>
        /// <remarks>This method is made public for testing purposes. Should normally not be called from outside this class.</remarks>
        public void SaveHeaderToExecutionContext(CompabilityInvocationContext context)
        {
            var headerValue = ExtractValueFromHeader(context);
            FulcrumApplication.Context.NexusTestContext = headerValue;
        }

        internal static string ExtractValueFromHeader(CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
#else
            var request = context?.RequestMessage;
#endif

            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
#if NETCOREAPP
            var headerValueExists = request.Headers.TryGetValue(Constants.NexusTestContextHeaderName, out var values);
#else
            var headerValueExists = request.Headers.TryGetValues(Constants.NexusTestContextHeaderName, out var values);
#endif
            if (!headerValueExists) return null;
            var valuesAsArray = values.ToArray();
            if (valuesAsArray.Length > 1)
            {
                var message = $"There was more than one {Constants.NexusTestContextHeaderName} headers: {string.Join(", ", valuesAsArray)}. Using the first one.";
                Log.LogWarning(message);
            }

            return valuesAsArray[0];
        }
    }
#if NETCOREAPP
    public static class SaveNexusTestContextExtension
    {
        public static IApplicationBuilder UseNexusSaveTestContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SaveNexusTestContext>();
        }
    }
#endif
}