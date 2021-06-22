#if NETCOREAPP
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic
{
    public class ResponseHandlerInMemory : ResponseHandlerBase
    {
        private static readonly ConcurrentDictionary<Guid, ResponseData> ResponsesByRequestId = new ConcurrentDictionary<Guid, ResponseData>();

        /// <inheritdoc />
        public ResponseHandlerInMemory(string urlFormat) : base(urlFormat)
        {
        }

        public override Task<ResponseData> GetResponseAsync(Guid requestId)
        {
            var response = ResponsesByRequestId.TryGetValue(requestId, out var responseData) ? responseData : AcceptedResponse(requestId);
            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public override Task AddResponse(RequestData requestData, ResponseData responseData)
        {
            // Serialize the response and make it available to the caller
            ResponsesByRequestId.TryAdd(requestData.Id, responseData);
            return Task.CompletedTask;
        }
    }
}
#endif