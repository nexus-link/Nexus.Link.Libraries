namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
 /* [TestClass]
    public class RestClientManyToOneRecursiveTest : TestBase
    {
        private const string ResourcePath = "http://example.se/Persons";
        private Person _person;
        private IManyToOne<Person, Guid> _parentChildrenClient;
        private IManyToOne<Person, Guid> _oneManyClient;


        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(RestClientTest).FullName);
            FulcrumApplication.Setup.ContextValueProvider = new SingleThreadValueProvider();
            HttpClientMock = new Mock<IHttpClient>();
            RestClient.HttpClient = HttpClientMock.Object;
            _parentChildrenClient = new ManyToOneCompleteRestClient<Person, Guid>(ResourcePath);
            _oneManyClient = new ManyToOneCompleteRestClient<Person, Guid>(ResourcePath, "One", "Many");
            _person = new Person()
            {
                GivenName = "Kalle",
                Surname = "Anka"
            };
        }

        [TestMethod]
        public async Task ReadChildren1Test()
        {
            await ReadChildrenTest(_parentChildrenClient, "Children");
        }

        [TestMethod]
        public async Task ReadChildren2Test()
        {
            await ReadChildrenTest(_oneManyClient, "Many");
        }
        public async Task ReadChildrenTest(IManyToOne<Person, Guid> restClient, string resourceName)
        {
            var parentId = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{parentId}/{resourceName}?limit={int.MaxValue}";
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
        public async Task ReadChildrenWithPaging1Test()
        {
            await ReadChildrenWithPagingTest(_parentChildrenClient, "Children");
        }

        [TestMethod]
        public async Task ReadChildrenWithPaging2Test()
        {
            await ReadChildrenWithPagingTest(_oneManyClient, "Many");
        }

        public async Task ReadChildrenWithPagingTest(IManyToOne<Person, Guid> restClient, string resourceName)
        {
            var parentId = Guid.NewGuid();
            var expectedUri = $"{ResourcePath}/{parentId}/{resourceName}?offset=0";
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
    }*/
}
