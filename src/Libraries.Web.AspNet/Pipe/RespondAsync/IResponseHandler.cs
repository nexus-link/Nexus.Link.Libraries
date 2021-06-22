#if NETCOREAPP
using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync
{
    public interface IResponseHandler : IGetActionResult
    {
        Task AddResponse(RequestData requestData, ResponseData responseData);
        Task<ResponseData> GetResponseAsync(Guid requestId);
        string GetResponseUrl(Guid requestId);
    }
}
#endif