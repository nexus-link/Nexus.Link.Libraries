#if NETCOREAPP
#else
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Azure.Web.AspNet.Telemetry;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Telemetry;
using System.Net.Http;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.Telemetry
{
    [TestClass]
    public class TrackAggregatedRequestUrlsTest
    {
        private Mock<ITelemetryHandler> _telemetryHandlerMock;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TrackAggregatedRequestUrlsTest).FullName);

            _telemetryHandlerMock = new Mock<ITelemetryHandler>();
            FulcrumApplication.Setup.TelemetryHandler = _telemetryHandlerMock.Object;
        }

        [TestMethod]
        [DataRow("api/v1/Persons/878/Invoices", "api/v1/Persons/*/Invoices", "Get")]
        [DataRow("api/v1/Persons/poipo8-989ijk-998unek/Invoices", "api/v1/Persons/*/Invoices", "Post")]
        [DataRow("api/v1/Persons/878", "api/v1/Persons/*", "Get")]
        [DataRow("api/v1/Persons/opkknw-ee3r3fe-vvrvfv", "api/v1/Persons/*", "Put")]
        [DataRow("api/v1/Persons/", "api/v1/Persons", "Get")]
        [DataRow("api/v1/Persons?from=2020-03-02", "api/v1/Persons", "Post")]
        public async Task SaveClientTenantOldPrefixSuccess(string path, string aggregatedOn, string method)
        {
            var url = $"https://localhost/{path}";
            var regexes = new Dictionary<string, Regex>
            {
                { "api/v1/Persons/*/Invoices", new Regex("/api/v1/Persons/[^/]+/Invoices") },
                { "api/v1/Persons/*", new Regex("/api/v1/Persons/[^/]+") },
                { "api/v1/Persons", new Regex("/api/v1/Persons") },
            };

            var aggregatedRequestUrls = new TrackAggregatedRequestUrls(regexes)
            {
                InnerHandler = new Mock<HttpMessageHandler>().Object
            };

            string result = null, httpMethod = null, actualPath = null;
            _telemetryHandlerMock
                .Setup(x => x.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()))
                .Callback((string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics) =>
                    {
                        result = properties["AggregatedOn"];
                        httpMethod = properties["HttpMethod"];
                        actualPath = properties["PathAndQuery"];
                    });

            var invoker = new HttpMessageInvoker(aggregatedRequestUrls);
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            await invoker.SendAsync(request, CancellationToken.None);

            Console.WriteLine($"{method} {aggregatedOn} ({actualPath})");

            Assert.AreEqual(aggregatedOn, result);
            Assert.AreEqual(method.ToUpperInvariant(), httpMethod.ToUpperInvariant());
        }
    }
}
#endif
