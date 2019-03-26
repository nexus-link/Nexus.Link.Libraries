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
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.ServiceAuthentication;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Nexus.Link.Libraries.Web.Tests.ServiceAuthentication
{
    [TestClass]
    public class ServiceAuthenticationHelperTest
    {
        private ServiceAuthenticationHelper _authenticationHelper;
        private Mock<IHttpClient> _httpClientMock;

        private ILeverConfiguration LeverConfiguration { get; set; }

        private static readonly Tenant Tenant = new Tenant("an-org", "some-env");
        private const string ClientName = "klaj";
        private const string Jwt = "fakejwt";

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ServiceAuthenticationHelperTest).FullName);

            _httpClientMock = new Mock<IHttpClient>();

            ServiceAuthenticationHelper.HttpClient = _httpClientMock.Object;
            ServiceAuthenticationHelper.ClearCache();
            _authenticationHelper = new ServiceAuthenticationHelper();
        }

        /// <summary>
        /// This is to show why we have a workaround in the casting in <see cref="ServiceAuthenticationHelper.GetAuthorizationForClientAsync"/>.
        /// We get "Cannot cast Newtonsoft.Json.Linq.JObject to Newtonsoft.Json.Linq.JToken" if we use jobject.Value
        /// </summary>
        [TestMethod]
        public void ShowWhyWeHaveToMakeWorkaroundInServiceAuthenticationHelper()
        {
            // From JObject implementation:
            // if ((object)token is U && typeof(U) != typeof(IComparable) && typeof(U) != typeof(IFormattable))
            //     return (U)(object)token;
            // JValue jvalue = (object)token as JValue;
            // if (jvalue == null)
            //     throw new InvalidCastException("Cannot cast {0} to {1}.".FormatWith((IFormatProvider)CultureInfo.InvariantCulture, (object)token.GetType(), (object)typeof(T)));

            var jobject = JObject.Parse(JObject.FromObject(new
            {
                x = new ClientAuthorizationSettings
                {
                    AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                    Username = "boom",
                    Password = "beat"
                }
            }
            ).ToString());
            Console.WriteLine("1: " + jobject);
            Console.WriteLine("2: " + jobject.Value<dynamic>("x")); // Ok
            Console.WriteLine("3: " + jobject.Value<JObject>("x")); // Ok
            Console.WriteLine("4: " + JsonConvert.DeserializeObject<ClientAuthorizationSettings>(jobject["x"].ToString()).AuthorizationType); // Ok
            try
            {
                Console.WriteLine("5: " + jobject.Value<ClientAuthorizationSettings>("x")); // This will crash :(
                Assert.Fail("Expected #5 to crash");
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e);
            }
        }

        [TestMethod]
        public async Task DefaultType()
        {
            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.IsNull(result);
        }

        private void SetupConfigMock(ClientAuthorizationSettings settings)
        {
            var conf = new JObject { { $"{ClientName}-authentication", JObject.FromObject(settings) } };
            LeverConfiguration = new MockLeverConfiguration(conf);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public async Task ExpectArray()
        {
            const string conf = "{\"shared-client-authentications\": {}}";
            var jObject = JObject.Parse(conf);
            LeverConfiguration = new MockLeverConfiguration(jObject);
            await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "advantage");
            Assert.Fail("Expected an exception");
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

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.IsNull(result.Token, result.Token); // Expect response, not exception
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

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.IsNull(result.Token, result.Token); // Expect response with null token, not exception
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

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.IsNull(result.Token, result.Token); // Expect response with null token, not exception
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

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.IsNull(result.Token);

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

            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
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
            var shared = new[]
            {
                new ClientAuthorizationSettings
                {
                    AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic,
                    Username = "x", Password = "y",
                    UseForClients = new []{ "another-client", "yet-another-client" }
                },
                new ClientAuthorizationSettings
                {
                    AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.BearerToken,
                    Token = "token",
                    UseForClients = new []{ "client-2", "client-3" }
                }
            };
            var conf = new JObject
            {
                { "shared-client-authentications", JArray.FromObject(shared) },
                { $"{ClientName}-authentication", JObject.FromObject(new ClientAuthorizationSettings { AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic, Username = "foo", Password = "bar" }) }
            };
            LeverConfiguration = new MockLeverConfiguration(conf);

            var result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, ClientName);
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
            Assert.AreEqual("foo:bar", Base64Decode(result.Token));
            result = await _authenticationHelper.GetAuthorizationForClientAsync(Tenant, LeverConfiguration, "another-client");
            Assert.AreEqual("basic", result.Type.ToLowerInvariant());
            Assert.AreEqual("x:y", Base64Decode(result.Token));
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
    }

    public class MockLeverConfiguration : ILeverConfiguration
    {
        /// <summary>
        /// This is where all the configuration values are stored
        /// </summary>
        public JObject JObject { get; internal set; }

        public MockLeverConfiguration(JObject jObject)
        {
            JObject = jObject;
        }

        /// <inheritdoc />
        public T Value<T>(object key)
        {
            return JObject.Value<T>(key);
        }

        /// <inheritdoc />
        public T MandatoryValue<T>(object key)
        {
            var value = Value<T>(key);
            return value;
        }
    }
}
