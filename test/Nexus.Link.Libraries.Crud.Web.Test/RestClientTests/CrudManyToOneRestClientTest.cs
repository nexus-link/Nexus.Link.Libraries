using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Crud.Web.Test.Support.Models;
using Nexus.Link.Libraries.Web.RestClientHelper;
#pragma warning disable 618

namespace Nexus.Link.Libraries.Crud.Web.Test.RestClientTests
{
    [TestClass]
    public class CrudManyToOneRestClientTest : TestBase
    {
        private const string ResourcePath = "http://example.se/Persons";
        private ICrudManyToOne<Address, Guid> _client;
        private Address _address;


        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(CrudManyToOneRestClientTest).FullName);
            HttpClientMock = new Mock<IHttpClient>();
            Libraries.Web.RestClientHelper.HttpSender.DefaultHttpClient = HttpClientMock.Object;
            var httpSender = new HttpSender(ResourcePath) {HttpClient = HttpClientMock.Object};
            _client = new CrudManyToOneRestClient<Address, Guid>(httpSender, "Person", "Addresses");
            _address = new Address()
            {
                Street = "Paradisäppelvägen 111",
                City = "Ankeborg"
            };
        }

        [TestMethod]
        public async Task ReadChildrenTest()
        {
            var parentId = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{parentId}/Addresses?limit={int.MaxValue}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => IsExpectedRequest(request, expectedUri, HttpMethod.Get)),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, new[] { _address }))
                .Verifiable();
            var addresses = await _client.ReadChildrenAsync(parentId);
            Assert.IsNotNull(addresses);
            var addressArray = addresses as Address[] ?? addresses.ToArray();
            Assert.AreEqual(1, addressArray.Length);
            Assert.AreEqual(_address, addressArray.FirstOrDefault());
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task ReadChildrenWithPagingTest()
        {
            var parentId = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{parentId}/Addresses?offset=0";
            var pageEnvelope = new PageEnvelope<Address>(0, PageInfo.DefaultLimit, null, new[] { _address });
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => IsExpectedRequest(request, expectedUri, HttpMethod.Get)),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, pageEnvelope))
                .Verifiable();
            var readPage = await _client.ReadChildrenWithPagingAsync(parentId, 0);
            Assert.IsNotNull(readPage?.Data);
            Assert.AreEqual(1, readPage.Data.Count());
            Assert.AreEqual(_address, readPage.Data.FirstOrDefault());
            HttpClientMock.Verify();
        }

        private static bool IsExpectedRequest(HttpRequestMessage request, string expectedUri, HttpMethod expectedMethod)
        {
            Assert.AreEqual(expectedUri, request.RequestUri.AbsoluteUri);
            return request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Get;
        }
    }
}
