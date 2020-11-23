using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Nexus.Link.Libraries.Web.Tests.Support
{
    internal class PropagateNexusTestHeaderForTest : Pipe.Outbound.PropagateNexusTestHeader
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
