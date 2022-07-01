using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Clients;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Nexus.Link.Libraries.Web.Tests.Clients
{
    [TestClass]
    public class ClientConfigTest
    {
        private ILeverConfiguration LeverConfiguration { get; set; }

        private const string ClientName = "my-page-adapter";
        private const string ClientName2 = "c2";
        private const string ClientNameNoAuth = "c3";
        private const string ClientName4 = "c4";

        private List<ClientConfiguration> _clientConfigurations;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ClientConfigTest).FullName);

            _clientConfigurations = new List<ClientConfiguration>
            {
                // Both request headers and authentication
                new ClientConfiguration
                {
                    Name = ClientName,
                    RequestHeaders = new Dictionary<string, string>
                    {
                        { "foo", "bar" }
                    },
                    Authentication = "auth"
                },
                // Request headers, but no authentication
                new ClientConfiguration
                {
                    Name = ClientNameNoAuth,
                    RequestHeaders = new Dictionary<string, string>
                    {
                        { "baz", "boz" }
                    }
                },
                // Authentication, but no request headers
                new ClientConfiguration
                {
                    Name = ClientName2,
                    Authentication = "auth"
                },
                // No authentication and no request headers
                new ClientConfiguration
                {
                    Name = ClientName4
                }
            };
            CreateLeverConfiguration();
        }

        private void CreateLeverConfiguration()
        {
            LeverConfiguration = new MockLeverConfiguration(JObject.FromObject(new
            {
                Clients = _clientConfigurations
            }));
        }

        private void SetupConfigMock(Dictionary<string, string> headers)
        {
            _clientConfigurations.First(x => x.Name == ClientName).RequestHeaders = headers;
            CreateLeverConfiguration();
        }

        [TestMethod]
        public void MultipleClients()
        {
            var configs = ClientConfigurationHelper.GetClientsConfigurations(LeverConfiguration);
            Assert.AreEqual(_clientConfigurations.Count, configs.Count);
            Assert.IsTrue(configs.ContainsKey(ClientName));
            Assert.IsTrue(configs.ContainsKey(ClientName2));
            Assert.IsTrue(configs.ContainsKey(ClientNameNoAuth));
            Assert.IsTrue(configs.ContainsKey(ClientName4));
        }

        [TestMethod]
        public void SingleHeader()
        {
            SetupConfigMock(new Dictionary<string, string>
            {
                { "foo", "bar" }
            });

            var config = ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, ClientName);
            Assert.IsNotNull(config?.RequestHeaders);
            Assert.AreEqual(1, config.RequestHeaders.Count);
            Assert.IsTrue(config.RequestHeaders.ContainsKey("foo"));
            Assert.AreEqual("bar", config.RequestHeaders["foo"]);
            Assert.AreEqual("auth", config.Authentication);
        }

        [TestMethod]
        public void EmptyHeaders()
        {
            SetupConfigMock(new Dictionary<string, string>());

            var config = ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, ClientName);
            Assert.IsNotNull(config?.RequestHeaders);
            Assert.AreEqual(0, config.RequestHeaders.Count);
        }

        [TestMethod]
        public void NullHeaders()
        {
            SetupConfigMock(null);

            var config = ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, ClientName);
            Assert.IsNotNull(config);
            Assert.IsNull(config.RequestHeaders);
        }

        [TestMethod]
        public void NoAuthentication()
        {
            SetupConfigMock(null);

            var config = ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, ClientNameNoAuth);
            Assert.IsNotNull(config);
            Assert.IsNull(config.Authentication);
        }

        [TestMethod]
        public void ConfigurationFromJsonFile()
        {
            // https://docs.nexus.link/docs/client-authentication-methods
            LeverConfiguration = new MockLeverConfiguration(JObject.Parse(File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Clients\\client-config.json")));

            var config = ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, "client-a");
            Assert.IsNotNull(config?.RequestHeaders);
            Assert.AreEqual(2, config.RequestHeaders.Count);
            Assert.IsTrue(config.RequestHeaders.ContainsKey("header-a"));
            Assert.IsTrue(config.RequestHeaders.ContainsValue("value-a"));
            Assert.IsTrue(config.RequestHeaders.ContainsKey("header-b"));
            Assert.IsTrue(config.RequestHeaders.ContainsValue("value-b"));
            Assert.AreEqual("auth", config.Authentication);

            config = ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, "client-no-auth");
            Assert.IsNotNull(config);
            Assert.IsNull(config.Authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumBusinessRuleException))]
        public void MultipleAuthenticationsFail()
        {
            SetupConfigMock(new Dictionary<string, string>
            {
                { "Authorization", "bar" }
            });

            ClientConfigurationHelper.GetConfigurationForClient(LeverConfiguration, ClientName);
        }
    }
}
