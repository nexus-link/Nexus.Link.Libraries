using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Web.Tests.Support
{
    internal class LogRequestAndResponseForTest : Pipe.Outbound.LogRequestAndResponse
    {
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await base.SendAsync(request, CancellationToken.None);
        }
    }
}
