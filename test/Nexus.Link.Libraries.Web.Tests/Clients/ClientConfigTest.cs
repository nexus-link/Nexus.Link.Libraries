﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
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

        private List<ClientConfiguration> _clientConfigurations;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ClientConfigTest).FullName);

            _clientConfigurations = new List<ClientConfiguration>
            {
                new ClientConfiguration
                {
                    Name = ClientName,
                    RequestHeaders = new Dictionary<string, string>
                    {
                        { "foo", "bar" }
                    },
                    Authentication = "auth"
                },
                new ClientConfiguration
                {
                    Name = ClientName2,
                    Authentication = "auth"
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
        public void Multipleclients()
        {
            var configs = ClientConfigurationHelper.GetClientsConfigurations(LeverConfiguration);
            Assert.AreEqual(_clientConfigurations.Count, configs.Count);
            Assert.IsTrue(configs.ContainsKey(ClientName));
            Assert.IsTrue(configs.ContainsKey(ClientName2));
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
        }
    }
}