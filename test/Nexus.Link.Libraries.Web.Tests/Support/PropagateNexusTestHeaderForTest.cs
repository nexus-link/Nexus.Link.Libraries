using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace Nexus.Link.Libraries.Web.Tests.Support
{
    internal class PropagateNexusTestHeaderForTest : PropagateNexusTestHeader
    {
        public PropagateNexusTestHeaderForTest()
        {
            InnerHandler = new Mock<DelegatingHandler>().Object;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await base.SendAsync(request, CancellationToken.None);
        }
    }
}
