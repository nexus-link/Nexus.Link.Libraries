using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 659

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    [TestClass]
    public class ValueTranslatorHttpSenderTest
    {
        private Mock<ITranslatorService> _translatorServiceMock;
        private const string ConsumerId = "in-1";
        private const string ProducerId = "out-1";
        private string _decoratedConsumerId;
        private List<string> DecoratedConsumerIds => new List<string> { _decoratedConsumerId };
        private string _decoratedProducerId;

        [TestInitialize]
        public void Initialize()
        {
            _decoratedConsumerId = $"(foo.id!~consumer!{ConsumerId})";
            _decoratedProducerId = $"(foo.id!~producer!{ProducerId})";
            FulcrumApplicationHelper.UnitTestSetup(typeof(ValueTranslatorHttpSenderTest).FullName);
            _translatorServiceMock = new Mock<ITranslatorService>();
            _translatorServiceMock
                .Setup(ts => ts.TranslateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, string> { { $"{_decoratedConsumerId}", ProducerId } });
            ValueTranslatorHttpSender.TranslatorService = _translatorServiceMock.Object;
        }

        [TestMethod]
        public async Task TranslateRelativeUrl()
        {
            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            await sender.SendRequestAsync(HttpMethod.Get, $"Foos/{_decoratedConsumerId}");
            Assert.AreEqual($"Foos/{ProducerId}", httpSenderMock.RelativeUrl);
        }

        [TestMethod]
        public async Task DoNotTranslateBody()
        {
            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            var inBody = new Foo { Id = _decoratedConsumerId, Name = "name" };
            await sender.SendRequestAsync(HttpMethod.Get, $"Foos/{ProducerId}", inBody);
            Assert.IsNotNull(httpSenderMock.ReceivedBody);
            var outBody = httpSenderMock.ReceivedBody as Foo;
            Assert.IsNotNull(outBody);
            Assert.AreEqual(ProducerId, outBody.Id);
            Assert.AreEqual(inBody.Name, outBody.Name);
        }

        [TestMethod]
        public async Task TranslateObjectBody()
        {
            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            var sentBody = new Foo { Id = _decoratedConsumerId, Name = "name" };
            var result = await sender.SendRequestAsync<Foo, Foo>(HttpMethod.Get, $"Foos/{ProducerId}", sentBody);
            Assert.IsNotNull(httpSenderMock.ReceivedBody);
            var receivedBody = httpSenderMock.ReceivedBody as Foo;
            Assert.IsNotNull(receivedBody);
            Assert.AreEqual(ProducerId, receivedBody.Id);
            Assert.AreEqual(sentBody.Name, receivedBody.Name);
            Assert.IsNotNull(result.Body);
            var resultBody = result.Body;
            Assert.IsNotNull(resultBody);
            VerifyFoo(resultBody);
        }

        [TestMethod]
        public async Task SendRequestAsync_TranslateUserId()
        {
            FulcrumApplication.Context.ValueProvider.SetValue("DecoratedUserIds", DecoratedConsumerIds);

            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            await sender.SendRequestAsync(HttpMethod.Get, $"Foos/{_decoratedConsumerId}");

            Assert.AreEqual(ProducerId, sender.TranslatedUserId_OnlyForUnitTests);
        }

        [TestMethod]
        public async Task SendAsync_TranslateUserId()
        {
            FulcrumApplication.Context.ValueProvider.SetValue("DecoratedUserIds", DecoratedConsumerIds);

            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            await sender.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"Foos/{_decoratedConsumerId}"));

            Assert.AreEqual(ProducerId, sender.TranslatedUserId_OnlyForUnitTests);
        }

        [TestMethod]
        public async Task TranslateListBody()
        {
            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            var sentBody = new Foo { Id = _decoratedConsumerId, Name = "name" };
            var result = await sender.SendRequestAsync<IEnumerable<Foo>, Foo>(HttpMethod.Get, $"Foos", sentBody);
            var resultBody = result.Body;
            Assert.IsNotNull(resultBody);
            var foo = resultBody.FirstOrDefault();
            Assert.IsNotNull(foo);
            VerifyFoo(foo);
        }

        [TestMethod]
        public async Task TranslatePageContent()
        {
            var httpSenderMock = new HttpSenderMock();
            var sender = new ValueTranslatorHttpSender(httpSenderMock, "producer");
            var inBody = new Foo { Id = _decoratedConsumerId, Name = "name" };
            var result = await sender.SendRequestAsync<PageEnvelope<Foo>, Foo>(HttpMethod.Get, $"Foos/{ProducerId}", inBody);
            Assert.IsNotNull(result?.Body);
            Assert.IsNotNull(result.Body.Data);
            Assert.IsTrue(result.Body.Data.Count() > 0);
            foreach (var foo in result.Body.Data)
            {
                VerifyFoo(foo);
            }
        }

        [TestMethod]
        public async Task TranslatorServiceIsNotCalledForIfNoTranslations()
        {
            _translatorServiceMock = new Mock<ITranslatorService>();
            _translatorServiceMock
                .Setup(ts => ts.TranslateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ITranslatorService should not be called if no translations"));
            ValueTranslatorHttpSender.TranslatorService = _translatorServiceMock.Object;

            var sender = new ValueTranslatorHttpSender(new HttpSenderMock(), "producer");
            var inBody = new Tenant("o", "e");
            // There are no translations in this request, so ITranslatorService.TranslateAsync should not be called
            await sender.SendRequestAsync(HttpMethod.Get, $"Tenants/{ProducerId}", inBody);
        }

        private void VerifyFoo(Foo foo)
        {
            Assert.AreEqual(_decoratedProducerId, foo.Id);
            Assert.AreEqual("out-name", foo.Name);
            Assert.AreEqual(_decoratedProducerId, foo.IdList[0]);
            Assert.AreEqual(_decoratedProducerId, foo.IdArray[0]);
        }

        private class HttpSenderMock : IHttpSender
        {
            public string RelativeUrl { get; private set; }
            public object ReceivedBody { get; private set; }

            /// <inheritdoc />
            public Uri BaseUri => new Uri("http://localhost");

            /// <inheritdoc />
            public IHttpSender CreateHttpSender(string relativeUrl)
            {
                throw new NotImplementedException();
            }

            public ServiceClientCredentials Credentials { get; }

            /// <inheritdoc />
            public async Task<TResponse> SendRequestThrowIfNotSuccessAsync<TResponse, TBody>(HttpMethod method, string relativeUrl, TBody body = default,
                Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
            {
                RelativeUrl = relativeUrl;
                var response = await SendRequestAsync<TResponse, TBody>(method, relativeUrl, body, customHeaders, cancellationToken);
                var result = await HttpSender.VerifySuccessAndReturnBodyAsync(response, cancellationToken);

                return result;
            }

            /// <inheritdoc />
            public Task SendRequestThrowIfNotSuccessAsync<TBody>(HttpMethod method, string relativeUrl, TBody body = default,
                Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
            {
                RelativeUrl = relativeUrl;
                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders = null,
                CancellationToken cancellationToken = default)
            {
                RelativeUrl = relativeUrl;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            /// <inheritdoc />
            public Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl, TBody body = default,
                Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
            {
                RelativeUrl = relativeUrl;
                ReceivedBody = body;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            /// <inheritdoc />
            public Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl, TBody body = default,
                Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
            {
                RelativeUrl = relativeUrl;
                ReceivedBody = body;
                var httpOperationResponse = new HttpOperationResponse<TResponse>();
                var foo = new Foo
                {
                    Id = ProducerId,
                    Name = "out-name"
                };
                foo.IdList.Add(ProducerId);
                foo.IdArray[0] = ProducerId;
                if (typeof(TResponse) == typeof(string))
                {
                    httpOperationResponse.Body = (TResponse)(object)ProducerId;
                }
                else if (typeof(TResponse) == typeof(Foo))
                {
                    httpOperationResponse.Body = (TResponse)(object)foo;
                }
                else if (typeof(TResponse) == typeof(IEnumerable<Foo>))
                {
                    httpOperationResponse.Body =
                        (TResponse)(object)new List<Foo> { foo };
                }
                else if (typeof(TResponse) == typeof(PageEnvelope<Foo>))
                {
                    httpOperationResponse.Body = (TResponse)(object)new PageEnvelope<Foo>(0, 1, 1, new List<Foo> { foo });
                }
                else
                {
                    httpOperationResponse.Body = default;
                }
                return Task.FromResult(httpOperationResponse);
            }

            /// <inheritdoc />
            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public string GetAbsoluteUrl(string relativeUrl)
            {
                throw new NotImplementedException();
            }
        }

        private class Foo : IUniquelyIdentifiable<string>
        {
            /// <inheritdoc />
            [TranslationConcept("foo.id")]
            public string Id { get; set; }

            [TranslationConcept("foo.id")]
            public List<string> IdList { get; } = new List<string>();

            [TranslationConcept("foo.id")]
            public string[] IdArray { get; } = new string[1];

            public string Name { get; set; }

            public DateTimeOffset When { get; } = DateTimeOffset.Now;

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (!(obj is Foo other)) return false;
                return other.Id == Id && other.Name == Name;
            }
        }
    }
}
