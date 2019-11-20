

using System;
using System.Net;
#if NETCOREAPP
using System.Net.Http.Formatting;
using System.Net.Http;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class ValueTranslatorFilterIntegrationTests
    {
        private HttpClient _httpClient;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ValueTranslatorFilterIntegrationTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
        }

        [TestMethod]
        public async Task OwinSimpleTest()
        {
            const string inId = "in-1";

            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();

            var response = await _httpClient.GetAsync($"/api/Foos/{inId}");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task ArgumentsAndResultAreTranslatedAsync()
        {
            const string inId = "in-1";
            const string outId = "out-1";

            // Mock a translator
            var translatorServiceMock = new Mock<ITranslatorService>();
            translatorServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>new Dictionary<string, string> {{$"(foo.id!~consumer!{inId})", outId}});

            TestStartup.TranslatorFactory = new TranslatorFactory(translatorServiceMock.Object, () => "consumer");
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();

            // Call method
            var inFoo = new Foo
            {
                Id = inId,
                Name = "name"
            };
            var response = await _httpClient.PutAsync($"http://localhost/api/Foos/{inId}", new ObjectContent<Foo>(inFoo, new JsonMediaTypeFormatter()));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(jsonString);
            var outFoo = JsonConvert.DeserializeObject<Foo>(jsonString);

            // Verify that the result has been translated
            Assert.IsNotNull(outFoo);
            Assert.AreEqual(outId, outFoo.Id);
        }
    }
}
#endif