#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    [TestClass]
    public class RespondAsyncFilterTests
    {
        private Mock<IRespondAsyncFilterSupport> _respondAsyncHandler;
        private ActionExecutingContext _actionExecutingContext;
        private DefaultHttpContext _httpContext;
        private Mock<Controller> _controller;
        private Mock<ActionExecutingContext> _actionExecutingContextMock;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(RespondAsyncFilterTests).FullName);
            _httpContext = new DefaultHttpContext();
            _httpContext.SetRequest("http://example.com");
            var actionContext = new ActionContext()
            {
                HttpContext = _httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            _controller = new Mock<Controller>();

            _actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                new Dictionary<string, object>(), _controller.Object);

            _actionExecutingContextMock = new Mock<ActionExecutingContext>();

            _respondAsyncHandler = new Mock<IRespondAsyncFilterSupport>();
            _respondAsyncHandler.Setup(rq =>
                    rq.EnqueueAsync(It.IsAny<HttpRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Guid.NewGuid()));
            _respondAsyncHandler.Setup(rq =>
                    rq.GetActionResultAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IActionResult)new NoContentResult()));
        }

        [DataTestMethod]
        [DataRow(false, RespondAsyncOpinionEnum.Indifferent, false)]
        [DataRow(false, RespondAsyncOpinionEnum.Never, false)]
        [DataRow(false, RespondAsyncOpinionEnum.Always, true)]
        [DataRow(true, RespondAsyncOpinionEnum.Indifferent, true)]
        [DataRow(true, RespondAsyncOpinionEnum.Never, false)]
        [DataRow(true, RespondAsyncOpinionEnum.Always, true)]
        public async Task RespectClientPreferencesAndMethodOpinion(bool clientPreferRespondAsync, RespondAsyncOpinionEnum methodOpinion, bool expectAccept)
        {
            // Arrange
            if (clientPreferRespondAsync) _httpContext.Request.Headers.Add(Constants.PreferHeaderName, Constants.PreferRespondAsyncHeaderValue);
            var methodInfo = GetMethodInfo(methodOpinion);
            var descriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = GetType().GetTypeInfo(),
                MethodInfo = methodInfo
            };
            _actionExecutingContext.ActionDescriptor = descriptor;
            var filter = new Pipe.Inbound.RespondAsyncFilter(_respondAsyncHandler.Object);
            var nextWasCalled = false;

            // Assert arrange
            Assert.IsNotNull(methodInfo);
            Assert.IsNotNull(_actionExecutingContext.ActionDescriptor);
            var cad = _actionExecutingContext.ActionDescriptor as ControllerActionDescriptor;
            Assert.IsNotNull(cad);

            // Act
            await filter.OnActionExecutionAsync(_actionExecutingContext, () =>
            {
                nextWasCalled = true;
                return Task.FromResult((ActionExecutedContext)null);
            });

            // Assert
            if (expectAccept)
            {
                Assert.IsNotNull(_actionExecutingContext.Result);
            }
            else
            {
                Assert.IsNull(_actionExecutingContext.Result);
                Assert.IsTrue(nextWasCalled);
            }

        }

        private MethodInfo GetMethodInfo(RespondAsyncOpinionEnum methodOpinion)
        {
            switch (methodOpinion)
            {
                case RespondAsyncOpinionEnum.Indifferent:
                    return GetType().GetMethod(nameof(AsyncOpinionIndifferent));
                case RespondAsyncOpinionEnum.Never:
                    return GetType().GetMethod(nameof(AsyncOpinionNever));
                case RespondAsyncOpinionEnum.Always:
                    return GetType().GetMethod(nameof(AsyncOpinionAlways));
                default:
                    throw new ArgumentOutOfRangeException(nameof(methodOpinion), methodOpinion, null);
            }
        }

        [RespondAsync(RespondAsyncOpinionEnum.Indifferent)]
        public void AsyncOpinionIndifferent()
        {

        }

        [RespondAsync(RespondAsyncOpinionEnum.Never)]
        public void AsyncOpinionNever()
        {

        }

        [RespondAsync(RespondAsyncOpinionEnum.Always)]
        public void AsyncOpinionAlways()
        {

        }
    }
}
#endif