using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Logging;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Any non-successful response will be thrown as a <see cref="FulcrumException"/>.
    /// </summary>
    public class ThrowFulcrumExceptionOnFail : DelegatingHandler
    {
        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                var fulcrumException = await ExceptionConverter.ToFulcrumExceptionAsync(response);
                if (fulcrumException == null) return response;
                Log.LogInformation(
                    $"OUT request-response {response.ToLogString()} was converted to a FulcrumException:\r{fulcrumException.ToLogString()}");
                throw fulcrumException;
            }
            catch (FulcrumException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FulcrumAssertionFailedException($"Request {request.ToLogString()} failed with exception of type {exception.GetType()}.", exception);
            }
        }
    }
}
