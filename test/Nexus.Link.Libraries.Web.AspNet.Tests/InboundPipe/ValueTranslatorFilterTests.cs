
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
#if NETCOREAPP
using System.Collections.Generic;
using System.Threading;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class ValueTranslatorFilterTests
    {

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ValueTranslatorFilterTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
        }

        [TestMethod]
        public async Task ArgumentsAndResultAreTranslatedAsync()
        {
            const string inId = "in-1";
            const string outId = "out-1";

            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>new Dictionary<string, string> {{$"(foo.id!~consumer!{inId})", outId}});

            // Prepare context
            var foosController = new FoosController();
            var inFoo = new Foo
            {
                Id = inId,
                Name = "name"
            };
            var controllerActionDescriptor = new ControllerActionDescriptor();
            controllerActionDescriptor.MethodInfo =
                foosController.GetType().GetMethod(nameof(foosController.UpdateAndReturnAsync));
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = controllerActionDescriptor
            };
            var executingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                new Dictionary<string, object> {{"id", inFoo.Id}, {"item", inFoo}}, foosController)
            {
                Result = new JsonResult(inFoo)
            };

            // Setup the filter
            var filter = new ValueTranslatorFilter(new TranslatorFactory(testServiceMock.Object, "consumer"));

            // Run the filter
            Assert.IsFalse(inFoo.Id.StartsWith("(foo.id!"));
            await filter.OnActionExecutionAsync(executingContext, () => Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(),
                foosController)));
            Assert.IsTrue(inFoo.Id.StartsWith("(foo.id!"));

            // Verify that the result has been translated
            var jsonResult = executingContext.Result as JsonResult;
            Assert.IsNotNull(jsonResult);
            var jObject = jsonResult.Value as JObject;
            Assert.IsNotNull(jObject);
            var outFoo = jObject.ToObject<Foo>();
            Assert.IsNotNull(outFoo);
            Assert.AreEqual(outId, outFoo.Id);
        }
    }
}
#endif