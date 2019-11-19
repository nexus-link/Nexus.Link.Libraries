using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Tests.Support.Models;
#pragma warning disable 659

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    [TestClass]
    public class ValueTranslatorHttpSenderTest
    {
        private Mock<ITranslatorService> _translatorServiceMock;
        private static string _consumerId = "in-1";
        private static string _producerId = "out-1";
        private string _decoratedConsumerId;
        private string _decoratedProducerId;

        [TestInitialize]
        public void Initialize()
        {
            _decoratedConsumerId = $"(person.id!~consumer!{_consumerId})";
            _decoratedProducerId = $"(person.id!~producer!{_producerId})";
            FulcrumApplicationHelper.UnitTestSetup(typeof(ValueTranslatorHttpSenderTest).FullName);
            _translatorServiceMock = new Mock<ITranslatorService>();
            _translatorServiceMock
                .Setup(ts => ts.TranslateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, string>{{$"{_decoratedConsumerId}", _producerId}});
        }

        [TestMethod]
        public async Task TranslateRelativeUrl()
        {
            var httpSender = new HttpSender();
            var translatorSetup = new TranslatorFactory(_translatorServiceMock.Object, "producer");
            var sender = new ValueTranslatorHttpSender(httpSender, translatorSetup);
            await sender.SendRequestAsync(HttpMethod.Get, $"Persons/{_decoratedConsumerId}");
            Assert.AreEqual($"Persons/{_producerId}", httpSender.RelativeUrl);
        }

        [TestMethod]
        public async Task TranslateBody()
        {
            var httpSender = new HttpSender();
            var translatorSetup = new TranslatorFactory(_translatorServiceMock.Object, "producer");
            var sender = new ValueTranslatorHttpSender(httpSender, translatorSetup);
            var inBody = new Foo {Id = _decoratedConsumerId, Name = "name"};
            await sender.SendRequestAsync(HttpMethod.Get, $"Persons/{_producerId}", inBody);
            Assert.IsNotNull(httpSender.Body);
            var outBody = httpSender.Body as Foo;
            Assert.IsNotNull(outBody);
            Assert.AreEqual(_producerId, outBody.Id);
            Assert.AreEqual(inBody.Name, outBody.Name);
        }

        private class HttpSender : IHttpSender
        {
            public string RelativeUrl { get; private set; }
            public object Body { get; private set; }

            /// <inheritdoc />
            public Uri BaseUri => new Uri("http://localhost");

            /// <inheritdoc />
            public Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders = null,
                CancellationToken cancellationToken = new CancellationToken())
            {
                RelativeUrl = relativeUrl;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            /// <inheritdoc />
            public Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl, TBody body = default(TBody),
                Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                RelativeUrl = relativeUrl;
                Body = body;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            /// <inheritdoc />
            public Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl, TBody body = default(TBody),
                Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                RelativeUrl = relativeUrl;
                Body = body;
                var httpOperationResponse = new HttpOperationResponse<TResponse>();
                if (typeof(string) == typeof(TResponse))
                {
                    httpOperationResponse.Body = (TResponse)(object)_producerId;
                }
                else
                {
                    httpOperationResponse.Body = default(TResponse);
                }
                return Task.FromResult(httpOperationResponse);
            }
        }

        private class Foo : IUniquelyIdentifiable<string>
        {
            /// <inheritdoc />
            [TranslationConcept("foo.id")]
            public string Id { get; set; }

            public string Name { get; set; }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (!(obj is Foo other)) return false;
                return other.Id == Id && other.Name == Name;
            }
        }
    }
}
