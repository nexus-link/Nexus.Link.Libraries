using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Tests.Support;

namespace Nexus.Link.Libraries.Web.Tests.Pipe
{
    [TestClass]
    public class BusinessApiOutPipeTests
    {

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(BusinessApiOutPipeTests).FullName);
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [TestMethod]
        public async Task Translated_UserId_Is_Propagated()
        {
            // Arrange
            HttpRequestHeaders actualHeaders = null;
            var sut = new AddTranslatedUserIdForTest
            {
                UnitTest_SendAsyncDependencyInjection = (req, cancellationToken) =>
                {
                    actualHeaders = req.Headers;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
            };

            const string translatedUserId = "t-123";
            FulcrumApplication.Context.ValueProvider.SetValue(Constants.TranslatedUserIdKey, translatedUserId);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            await sut.SendAsync(request);

            // Assert
            Assert.IsNotNull(actualHeaders);
            Assert.IsTrue(actualHeaders.TryGetValues(Constants.NexusTranslatedUserIdHeaderName, out var header));
            Assert.AreEqual(translatedUserId, header.First());
        }

        /// <summary>
        /// Given that "NexusUserAuthorization" is setup on context, we expect it propagated as a header
        /// </summary>
        [TestMethod]
        public async Task User_Authorization_Is_Propagated()
        {
            // Arrange
            HttpRequestHeaders actualHeaders = null;
            var sut = new AddUserAuthorizationForTest
            {
                UnitTest_SendAsyncDependencyInjection = (req, cancellationToken) =>
                {
                    actualHeaders = req.Headers;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
            };

            const string userAuthorization = "eymannen";
            FulcrumApplication.Context.ValueProvider.SetValue(Constants.NexusUserAuthorizationKeyName, userAuthorization);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            await sut.SendAsync(request);

            // Assert
            Assert.IsNotNull(actualHeaders);
            Assert.IsTrue(actualHeaders.TryGetValues(Constants.NexusUserAuthorizationHeaderName, out var header));
            Assert.AreEqual(userAuthorization, header.First());
        }
    }
}
