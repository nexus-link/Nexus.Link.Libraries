using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Clients;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.ServiceAuthentication;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

// TODO: Test PostContentBody
namespace Nexus.Link.Libraries.Web.Tests.ServiceAuthentication
{
    [TestClass]
    public class ServiceAuthenticationHelperVersion2Test
    {
        private ServiceAuthenticationHelper _authenticationHelper;
        private Mock<IHttpClient> _httpClientMock;

        private ILeverConfiguration LeverConfiguration { get; set; }

        private static readonly Tenant Tenant = new Tenant("the-org", "the-env");
        private const string ClientName = "my-page-adapter";
        private const string Jwt = "somejwt";

        private List<ClientConfiguration> _clientConfigurations;
        private List<ClientAuthorizationSettings> _authentications;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ServiceAuthenticationHelperVersion2Test).FullName);

            _httpClientMock = new Mock<IHttpClient>();

            ServiceAuthenticationHelper.HttpClient = _httpClientMock.Object;
            ServiceAuthenticationHelper.ClearCache();
            _authenticationHelper = new ServiceAuthenticationHelper();

            _clientConfigurations = new List<ClientConfiguration>
            {
                new ClientConfiguration
                {
                    Name = ClientName,
                    Authentication = "Default"
                }
            };
            _authentications = new List<ClientAuthorizationSettings>{
                new ClientAuthorizationSettings
                {
                    Id = "Default",
                    AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                    Username = "username",
                    Password = "password"
                }
            };
            CreateLeverConfiguration();
        }

        private void CreateLeverConfiguration()
        {
            LeverConfiguration = new MockLeverConfiguration(JObject.FromObject(new
            {
                Clients = _clientConfigurations,
                Authentications = _authentications
            }));
        }

        private void SetupConfigMock(ClientAuthorizationSettings settings)
        {
            var authenticationId = Guid.NewGuid().ToString();
            _clientConfigurations.First(x => x.Name == ClientName).Authentication = authenticationId;
            settings.Id = authenticationId;
            _authentications.Add(settings);
            CreateLeverConfiguration();
        }

        [TestMethod]
        public async Task DefaultType()
        {
            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "unknown-client");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task BearerTokenSuccess()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.BearerToken,
                Token = Jwt
            });
            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.IsNotNull(result, JsonConvert.SerializeObject(LeverConfiguration, Formatting.Indented));
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(Jwt, result.Token, result.Token);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task JwtFromUrlNoUrl()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostBody = "{}",
                ResponseTokenJsonPath = "AccessToken"
            });
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task JwtFromUrlNoBody()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                ResponseTokenJsonPath = "AccessToken"
            });
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task JwtFromUrlNoResponseTokenPath()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
            });
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
        }

        [TestMethod]
        public async Task JwtFromUrlSuccess()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "data.AccessToken"
            });

            _httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
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

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(Jwt, result.Token, result.Token);
        }

        [TestMethod]
        public async Task JwtFromUrlBadResponseFromException()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "AccessToken"
            });

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
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
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "AccessToken"
            });

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
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
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "-",
                ResponseTokenJsonPath = "AccessToken"
            });

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("not json") // This will cause exception when parsed
                });

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
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
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl,
                PostUrl = "http://localhost",
                PostBody = "{}",
                ResponseTokenJsonPath = "data.AccessToken"
            });

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            try
            {
                await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
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

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual(Jwt, result.Token);
        }

        [TestMethod]
        public async Task TestBasicAuthSuccess()
        {
            const string username = "qwerty";
            const string password = "1234";

            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                Username = username,
                Password = password
            });

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
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
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                Password = "-"
            });
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumException), AllowDerivedTypes = true)]
        public async Task TestBasicAuthNoPassword()
        {
            SetupConfigMock(new ClientAuthorizationSettings
            {
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                Username = "-"
            });
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
        }

        [TestMethod]
        public async Task TestSharedSettings()
        {
            _authentications.Add(
                new ClientAuthorizationSettings
                {
                    Id = "Basic2",
                    AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                    Username = "foo",
                    Password = "bar"
                });
            _authentications.Add(
                new ClientAuthorizationSettings
                {
                    Id = "Basic3",
                    AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                    Username = "x",
                    Password = "y"
                });
            _authentications.Add(new ClientAuthorizationSettings
            {
                Id = "Static1",
                AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.BearerToken,
                Token = "token"
            });
            _clientConfigurations.Add(new ClientConfiguration { Name = "another-client", Authentication = "Basic2" });
            _clientConfigurations.Add(new ClientConfiguration { Name = "yet-another-client", Authentication = "Basic3" });
            _clientConfigurations.Add(new ClientConfiguration { Name = "client-2", Authentication = "Static1" });
            _clientConfigurations.Add(new ClientConfiguration { Name = "client-3", Authentication = "Static1" });
            CreateLeverConfiguration();

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
            Assert.AreEqual("username:password", Base64Decode(result.Token));
            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "another-client");
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
            Assert.AreEqual("foo:bar", Base64Decode(result.Token));
            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "yet-another-client");
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
            Assert.AreEqual("x:y", Base64Decode(result.Token));
            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "client-2");
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual("token", result.Token);
            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "client-3");
            Assert.AreEqual("bearer", result.Type.ToLowerInvariant());
            Assert.AreEqual("token", result.Token);
            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "unknown-client");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ConfigurationFromJsonFile()
        {
            // https://docs.nexus.link/docs/client-authentication-methods
            LeverConfiguration = new MockLeverConfiguration(JObject.Parse(File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\ServiceAuthentication\\auth-config.json")));

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "client-a");
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
            Assert.AreEqual("foo:bar", Base64Decode(result.Token));
        }

    }
}
