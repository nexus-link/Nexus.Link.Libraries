    #if NETCOREAPP
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic
{
    public abstract class RequestExecutorBase : IRequestExecutor
    {
        public static string IsRunningAsynchronouslyHeader { get; protected set; } = "X-Is-Running-Asynchronously";

        public HttpClient HttpClient { get; protected set; }

        protected RequestExecutorBase()
        {
            HttpClient = HttpClientFactory.Create();
        }

        public virtual bool IsRunningAsynchronously(HttpRequest request)
        {
            return request.Headers.ContainsKey(IsRunningAsynchronouslyHeader);
        }

        /// <inheritdoc />
        public virtual async Task<ResponseData> 
            ExecuteRequestAsync(RequestData requestData, CancellationToken cancellationToken = default)
        {
            ResponseData responseData;
            requestData.Headers.Add(IsRunningAsynchronouslyHeader, "TRUE");
            var requestMessage = requestData.ToHttpRequestMessage();
            try
            {
                var response = await HttpClient.SendAsync(requestMessage, cancellationToken);
                responseData = await new ResponseData().FromAsync(response);
            }
            catch (Exception e)
            {
                responseData = new ResponseData().From(e);
            }

            // Serialize the response and make it available to the caller
            return responseData;
        }
    }
}
#endif