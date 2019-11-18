using System;
using System.Linq;
using System.Net;
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
    public class CrudRestClientTests : TestBase
    {
        private const string ResourcePath = "http://example.se/Persons";
        private ICrud<Person, Guid> _client;
        private Person _person;


        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(CrudRestClientTests).FullName);
            HttpClientMock = new Mock<IHttpClient>();
            Libraries.Web.RestClientHelper.HttpSender.HttpClient = HttpClientMock.Object;
            _client = new CrudRestClient<Person, Guid>(new HttpSender(ResourcePath));
            _person = new Person()
            {
                GivenName = "Kalle",
                Surname = "Anka"
            };
        }

        [TestMethod]
        public async Task CreateAndReturnTest()
        {
            var expectedUri = $"{ResourcePath}/ReturnCreated";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Post),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, _person))
                .Verifiable();
            var person = await _client.CreateAndReturnAsync(_person);
            Assert.AreEqual(_person, person);
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task CreateTest()
        {
            var id = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Post),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, id))
                .Verifiable();
            var createdId = await _client.CreateAsync(_person);
            Assert.AreEqual(id, createdId);
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task ReadTest()
        {
            var id = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{id}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Get),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, _person))
                .Verifiable();
            var person = await _client.ReadAsync(id);
            Assert.AreEqual(_person, person);
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task ReadAllTest()
        {
            var expectedUri = $"{ResourcePath}?limit={int.MaxValue}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Get),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, new[] { _person }))
                .Verifiable();
            var persons = await _client.ReadAllAsync();
            Assert.IsNotNull(persons);
            var personArray = persons as Person[] ?? persons.ToArray();
            Assert.AreEqual(1, personArray.Length);
            Assert.AreEqual(_person, personArray.FirstOrDefault());
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task ReadAllWithPagingTest()
        {
            var expectedUri = $"{ResourcePath}?offset=0";
            var pageEnvelope = new PageEnvelope<Person>(0, PageInfo.DefaultLimit, null, new[] { _person });
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Get),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, pageEnvelope))
                .Verifiable();
            var page = await _client.ReadAllWithPagingAsync(0);
            Assert.IsNotNull(page?.Data);
            Assert.AreEqual(1, page.Data.Count());
            Assert.AreEqual(_person, page.Data.FirstOrDefault());
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task UpdateAndReturnTest()
        {
            var id = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{id}/ReturnUpdated";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Put),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, _person))
                .Verifiable();
            var person = await _client.UpdateAndReturnAsync(id, _person);
            Assert.AreEqual(_person, person);
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task UpdateTest()
        {
            var id = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{id}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Put),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r))
                .Verifiable();
            await _client.UpdateAsync(id, _person);
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            var id = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{id}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Delete),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r))
                .Verifiable();
            await _client.DeleteAsync(id);
            HttpClientMock.Verify();
        }

        [TestMethod]
        public async Task DeleteAllTest()
        {
            var expectedUri = $"{ResourcePath}";
            HttpClientMock.Setup(client => client.SendAsync(
                    It.Is<HttpRequestMessage>(request => request.RequestUri.AbsoluteUri == expectedUri && request.Method == HttpMethod.Delete),
                    CancellationToken.None))
                .ReturnsAsync((HttpRequestMessage r, CancellationToken c) => CreateResponseMessage(r, HttpStatusCode.NoContent, null))
                .Verifiable();
            await _client.DeleteAllAsync();
            HttpClientMock.Verify();
        }
    }
}
