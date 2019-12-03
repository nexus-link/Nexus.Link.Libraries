
using Newtonsoft.Json;
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
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class ValueTranslatorFilterUnitTests
    {

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ValueTranslatorFilterUnitTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
        }

        [TestMethod]
        public void JsonDeserialize()
        {
            var inFoo = new Foo
            {
                Id = "id", Name = "name"
            };
            var json = JsonConvert.SerializeObject(inFoo);
            var outFoo = JsonConvert.DeserializeObject<Foo>(json);
            Assert.AreEqual(typeof(Foo), outFoo.GetType());
        }

        [TestMethod]
            public async Task ArgumentsAreDecoratedAsync()
            {

            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>new Dictionary<string, string> {{$"(foo.id!~consumer!{Foo.ConsumerId1})", Foo.ProducerId1}});

            // Prepare context
            var foosController = new FoosController();
            var inFoo = new Foo
            {
                Id = Foo.ConsumerId1,
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
                new Dictionary<string, object> {{"id", inFoo.Id}, {"item", inFoo}}, foosController);

            // Setup the filter
            var filter = new ValueTranslatorFilter(new TranslatorFactory(testServiceMock.Object, Foo.ConsumerName));

            // Run the filter
            Assert.IsFalse(inFoo.Id.StartsWith("(foo.id!"));
            await filter.OnActionExecutionAsync(executingContext, () => Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(),
                foosController)));
            Assert.IsTrue(inFoo.Id.StartsWith("(foo.id!"));
        }

        [TestMethod]
        public async Task ResultIsTranslatedAsync()
        {
            var decoratedProducerId1 = Translator.Decorate(Foo.IdConceptName, Foo.ProducerName, Foo.ProducerId1);
            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(),It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>new Dictionary<string, string> {{decoratedProducerId1, Foo.ConsumerId1}});

            // Prepare context
            var foosController = new FoosController();
            var foo = new Foo
            {
                Id = decoratedProducerId1,
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
            var executingContext = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(foo), foosController);

            // Setup the filter
            var filter = new ValueTranslatorFilter(new TranslatorFactory(testServiceMock.Object, Foo.ConsumerName));

            // Run the filter
            await filter.OnResultExecutionAsync(executingContext, () => Task.FromResult(new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), executingContext.Result, foosController)));

            // Verify that the result has been translated
            var objectResult = executingContext.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            var outFoo = objectResult.Value as Foo;
            Assert.IsNotNull(outFoo);
            Assert.AreEqual(Foo.ConsumerId1, outFoo.Id);
        }
    }
}
#endif