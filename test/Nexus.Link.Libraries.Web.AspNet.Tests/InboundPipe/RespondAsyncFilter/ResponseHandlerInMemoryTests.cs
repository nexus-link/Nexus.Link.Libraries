#if NETCOREAPP
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    [TestClass]
    public class ResponseHandlerInMemoryTests
    {
        private const string UrlFormat = "http://example.com/Request/{0]/Response";
        private IResponseHandler _responseHandler;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ResponseHandlerInMemoryTests).FullName);
            _responseHandler = new ResponseHandlerInMemory(UrlFormat);
        }

        [TestMethod]
        public async Task ResponseIsStoredAndCanBeRetrieved()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
            var requestData = new RequestData
            {
                Id = Guid.NewGuid()
            };
            var responseData = new ResponseData
            {
                StatusCode = expectedStatusCode
            };
            await _responseHandler.AddResponse(requestData, responseData);
            var response = await _responseHandler.GetResponseAsync(requestData.Id);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }
    }
}
#endif