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

namespace Nexus.Link.Libraries.Crud.Web.Test.RestClientTests
{
    [TestClass]
    public class CrudManyToOneRecursiveRestClient2Test : TestBase
    {
        private const string ResourcePath = "http://example.se";
        private Person _person;
        private ICrudManyToOne<Person, Guid> _oneManyClient;


        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(CrudManyToOneRecursiveRestClientTest).FullName);
            HttpClientMock = new Mock<IHttpClient>();
            HttpSender.DefaultHttpClient = HttpClientMock.Object;
            var httpSender = new HttpSender(ResourcePath) {HttpClient = HttpClientMock.Object};
            _oneManyClient = new CrudManyToOneRestClient2<Person, Guid>(httpSender, "Persons", "Many");
            _person = new Person()
            {
                GivenName = "Kalle",
                Surname = "Anka"
            };
        }

        [TestMethod]
        public async Task ReadChildrenTest()
        {
            await ReadChildrenTest(_oneManyClient, "Many");
        }

        public async Task ReadChildrenTest(ICrudManyToOne<Person, Guid> restClient, string resourceName)
        {
            var parentId = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/Persons/{parentId}/{resourceName}?limit={int.MaxValue}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Get),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, new[] { _person }))
                .Verifiable();
            var persons = await restClient.ReadChildrenAsync(parentId);
            Assert.IsNotNull(persons);
            var personArray = persons as Person[] ?? persons.ToArray();
            Assert.AreEqual(1, personArray.Length);
            Assert.AreEqual(_person, personArray.FirstOrDefault());
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task ReadChildrenWithPagingTest()
        {
            await ReadChildrenWithPagingTest(_oneManyClient, "Many");
        }

        public async Task ReadChildrenWithPagingTest(ICrudManyToOne<Person, Guid> restClient, string resourceName)
        {
            var parentId = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/Persons/{parentId}/{resourceName}?offset=0";
            var pageEnvelope = new PageEnvelope<Person>(0, PageInfo.DefaultLimit, null, new[] { _person });
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Get),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, pageEnvelope))
                .Verifiable();
            var readPage = await restClient.ReadChildrenWithPagingAsync(parentId, 0);
            Assert.IsNotNull(readPage?.Data);
            Assert.AreEqual(1, readPage.Data.Count());
            Assert.AreEqual(_person, readPage.Data.FirstOrDefault());
            HttpClientMock.Verify();
        }
    }
}
