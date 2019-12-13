#if NETCOREAPP
using System;
using System.Net;
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
using Nexus.Link.Libraries.Core.Error.Logic;
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
            // Mock a translator
            var translatorServiceMock = new Mock<ITranslatorService>();
            var decoratedProducerId1 = Translator.Decorate(Foo.IdConceptName, Foo.ProducerName, Foo.ProducerId1);
            translatorServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>new Dictionary<string, string> {{decoratedProducerId1, Foo.ConsumerId1}});
            
            TestStartup.TranslatorService = translatorServiceMock.Object;
            TestStartup.GetTranslatorClientName = () => Foo.ConsumerName;
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();

            var response = await _httpClient.GetAsync($"/api/Foos/{Foo.ConsumerId1}");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task ArgumentsAndResultAreTranslatedAsync()
        {

            // Mock a translator
            var translatorServiceMock = new Mock<ITranslatorService>();
            var decoratedProducerId1 = Translator.Decorate(Foo.IdConceptName, Foo.ProducerName, Foo.ProducerId1);
            translatorServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>new Dictionary<string, string> {{decoratedProducerId1, Foo.ConsumerId1}});

            TestStartup.TranslatorService = translatorServiceMock.Object;
            TestStartup.GetTranslatorClientName = () => Foo.ConsumerName;            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();

            // Call method
            var inFoo = new Foo
            {
                Id = Foo.ConsumerId1,
                Name = "name"
            };
            var response = await _httpClient.PutAsync($"http://localhost/api/Foos/{Foo.ConsumerId1}", new ObjectContent<Foo>(inFoo, new JsonMediaTypeFormatter()));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var jsonString = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(jsonString));
            Console.WriteLine(jsonString);
            var outFoo = JsonConvert.DeserializeObject<Foo>(jsonString);

            // Verify that the result has been translated
            Assert.IsNotNull(outFoo);
            Assert.AreEqual(Foo.ConsumerId1, outFoo.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumResourceException))]
        public async Task TranslatorResourceException()
        {

            // Mock a translator
            var translatorServiceMock = new Mock<ITranslatorService>();
            var decoratedProducerId1 = Translator.Decorate(Foo.IdConceptName, Foo.ProducerName, Foo.ProducerId1);
            translatorServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new FulcrumResourceException("Fail"));

            TestStartup.TranslatorService = translatorServiceMock.Object;
            TestStartup.GetTranslatorClientName = () => Foo.ConsumerName;
            
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();

            // Call method
            var inFoo = new Foo
            {
                Id = Foo.ConsumerId1,
                Name = "name"
            };
            await _httpClient.PutAsync($"http://localhost/api/Foos/{Foo.ConsumerId1}", new ObjectContent<Foo>(inFoo, new JsonMediaTypeFormatter()));
        }
    }
}
#endif