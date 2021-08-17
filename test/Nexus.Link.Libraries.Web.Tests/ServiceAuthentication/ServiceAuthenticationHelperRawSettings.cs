using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.ServiceAuthentication;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Nexus.Link.Libraries.Web.Tests.ServiceAuthentication
{
    [TestClass]
    public class ServiceAuthenticationHelperRawSettings

    {
        private ServiceAuthenticationHelper _authenticationHelper;
        private Mock<IHttpClient> _httpClientMock;

        private static readonly Tenant Tenant = new Tenant("the-org", "the-env");
        private const string ClientName = "my-page-adapter";
        private const string Jwt = "somejwt";

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ServiceAuthenticationHelperVersion2Test).FullName);

            _httpClientMock = new Mock<IHttpClient>();

            ServiceAuthenticationHelper.HttpClient = _httpClientMock.Object;
            ServiceAuthenticationHelper.ClearCache();
            _authenticationHelper = new ServiceAuthenticationHelper();
        }


        [TestMethod]
        public async Task BearerTokenSuccess()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.BearerToken,
                Token = Jwt
            };
            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
            Assert.IsNotNull(result, JsonConvert.SerializeObject(auth, Formatting.Indented));
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(Jwt, result.Token, result.Token);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task JwtFromUrlNoUrl()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostBody = "{}",
                ResponseTokenJsonPath = "AccessToken"
            };
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task JwtFromUrlNoBody()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                ResponseTokenJsonPath = "AccessToken"
            };
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task JwtFromUrlNoResponseTokenPath()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
            };
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
        }

        [TestMethod]
        public async Task JwtFromUrlSuccess()
        {
            const string expectedContentType = "xx-application/json";
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                PostContentType = expectedContentType,
                ResponseTokenJsonPath = "data.AccessToken"
            };

            string contentType = null;
            _httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage request, CancellationToken cancellationToken) =>
                {
                    contentType = request.Content.Headers.ContentType.MediaType;
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JObject.FromObject(new
                    {
                        data = new
                        {
                            AccessToken = Jwt
                        }
                    }).ToString())
                });

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(Jwt, result.Token, result.Token);
            Assert.AreEqual(expectedContentType, contentType);
        }

        [TestMethod]
        public async Task JwtFromUrlBadResponseFromException()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "AccessToken"
            };

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e is FulcrumAssertionFailedException, $"Unexpected exception: {e.GetType().FullName}");
            }
        }

        [TestMethod]
        public async Task JwtFromUrlBadResponseFromBadStatusCode()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "AccessToken"
            };

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e is FulcrumAssertionFailedException, $"Unexpected exception: {e.GetType().FullName}");
            }
        }

        [TestMethod]
        public async Task JwtFromUrlBadResponseFromBadInput()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "-",
                ResponseTokenJsonPath = "AccessToken"
            };

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("not json") // This will cause exception when parsed
                });

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e is FulcrumAssertionFailedException, $"Unexpected exception: {e.GetType().FullName}");
            }
        }

        [TestMethod]
        public async Task JwtFromUrlSuccessAfterBadResponse()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "data.AccessToken"
            };

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e is FulcrumAssertionFailedException, $"Unexpected exception: {e.GetType().FullName}");
            }

            // Now, setup a successful response and make sure the bad response is not cached
            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JObject.FromObject(new
                    {
                        data = new
                        {
                            AccessToken = Jwt
                        }
                    }).ToString())
                });

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(Jwt, result.Token);
        }

        [TestMethod]
        public async Task TestBasicAuthSuccess()
        {
            const string username = "qwerty";
            const string password = "1234";

            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                Username = username,
                Password = password
            };

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
            Assert.AreEqual($"{username}:{password}", Base64Decode(result.Token));
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
        }

        private static string Base64Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task TestBasicAuthNoUsername()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                Password = "-"
            };
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task TestBasicAuthNoPassword()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                Username = "-"
            };
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
        }

        [TestMethod]
        public async Task PlatformServiceSuccess()
        {
            var tokenRefresherMock = new Mock<ITokenRefresher>();
            const string platformJwt = "platform-dancing";
            tokenRefresherMock
                .Setup(x => x.GetJwtTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthenticationToken { Type = "Bearer", AccessToken = platformJwt, ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) });

            var helper = new ServiceAuthenticationHelper(tokenRefresherMock.Object);

            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.NexusPlatformService
            };
            var result = await helper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);

            Assert.IsNotNull(result, JsonConvert.SerializeObject(auth, Formatting.Indented));
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(platformJwt, result.Token, result.Token);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task PlatformServiceFailsIfNoTokenRefresher()
        {
            var auth = new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.NexusPlatformService
            };
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, auth, ClientName);
        }
    }
}
