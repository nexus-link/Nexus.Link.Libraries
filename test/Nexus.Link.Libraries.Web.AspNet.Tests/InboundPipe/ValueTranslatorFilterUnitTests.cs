using System;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using System.Collections.Generic;
using System.Threading;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Translation;

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http;
using Microsoft.Rest;
using System.Net;
using System.Net.Http;
using System.Text;
#endif

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
            var tenant = new Tenant("o", "e");
            var inFoo = new Foo
            {
                Id = "id",
                Name = "name",
                Tenant = tenant
            };
            var json = JsonConvert.SerializeObject(inFoo);
            var outFoo = JsonConvert.DeserializeObject<Foo>(json);
            Assert.AreEqual(typeof(Foo), outFoo.GetType());
            Assert.AreEqual(tenant, outFoo.Tenant, "This requires setters on Tenant");
        }

        [TestMethod]
        public async Task ArgumentsAreDecoratedAsync()
        {

            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Dictionary<string, string> { { $"(foo.id!~consumer!{Foo.ConsumerId1})", Foo.ProducerId1 } });

            // Prepare context
            var inFoo = new Foo
            {
                Id = Foo.ConsumerId1,
                Name = "name"
            };
            var parameters = new Dictionary<string, object> { { "id", inFoo.Id }, { "item", inFoo }, { "token", default(CancellationToken) } };

#if NETCOREAPP
            var foosController = new FoosController();
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
                parameters, foosController);
#else
            var contextMock = CreateExecutingContext(parameters, nameof(FoosController.UpdateAndReturnAsync));
#endif

            // Setup the filter
            var filter = new ValueTranslatorFilter(testServiceMock.Object, () => Foo.ConsumerName);

            // Run the filter
            Assert.IsFalse(inFoo.Id.StartsWith("(foo.id!"));
#if NETCOREAPP
            await filter.OnActionExecutionAsync(executingContext, () => Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(),
                foosController)));
#else
            await filter.OnActionExecutingAsync(contextMock, new CancellationToken());
#endif
            Assert.IsTrue(inFoo.Id.StartsWith("(foo.id!"), inFoo.Id);
        }

        [TestMethod]
        public async Task ResultIsTranslatedAsync()
        {
            var decoratedProducerId1 = Translator.Decorate(Foo.IdConceptName, Foo.ProducerName, Foo.ProducerId1);
            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Dictionary<string, string> { { decoratedProducerId1, Foo.ConsumerId1 } });

            // Prepare context
            var foosController = new FoosController();
            var foo = new Foo
            {
                Id = decoratedProducerId1,
                Name = "name"
            };
#if NETCOREAPP
            var controllerActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = foosController.GetType().GetMethod(nameof(foosController.UpdateAndReturnAsync))
            };
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = controllerActionDescriptor
            };
            var executingContext = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(foo), foosController);

#else
            var contextMock = CreateExecutedContextWithStatusCode(HttpStatusCode.OK, foo);
#endif

            // Setup the filter
            var filter = new ValueTranslatorFilter(testServiceMock.Object, () => Foo.ConsumerName);

            // Run the filter
#if NETCOREAPP
            await filter.OnResultExecutionAsync(executingContext, () => Task.FromResult(new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), executingContext.Result, foosController)));
#else
            await filter.OnActionExecutedAsync(contextMock, new CancellationToken());
#endif

#if NETCOREAPP
            var objectResult = executingContext.Result as ObjectResult;
#else
            var objectResult = contextMock.Response.Content.AsString();
#endif

            // Verify that the result has been translated
            Assert.IsNotNull(objectResult);
#if NETCOREAPP
            var outFoo = objectResult.Value as Foo;
            if (outFoo == null && objectResult.Value is JObject jObject) outFoo = jObject.ToObject<Foo>();
#else
            var outFoo = JsonConvert.DeserializeObject<Foo>(objectResult);
#endif
            Assert.IsNotNull(outFoo);
            Assert.AreEqual(Foo.ConsumerId1, outFoo.Id);
        }

        [TestMethod]
        public async Task TranslatorServiceIsNotCalledForIfNoTranslations()
        {

            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ITranslatorService should not be called if no translations"));

#if NETCOREAPP
            var foosController = new FoosController();
            var controllerActionDescriptor = new ControllerActionDescriptor();
            controllerActionDescriptor.MethodInfo = foosController.GetType().GetMethod(nameof(foosController.ServiceTenant));
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = controllerActionDescriptor
            };
            var executingContext = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(FulcrumApplication.Setup.Tenant), foosController);
#else
            var contextMock = CreateExecutedContextWithStatusCode(HttpStatusCode.OK, FulcrumApplication.Setup.Tenant);
#endif

            // Setup the filter
            var filter = new ValueTranslatorFilter(testServiceMock.Object, () => Foo.ConsumerName);

            // Run the filter
            // There are no translations, so ITranslatorService.TranslateAsync should not be called
#if NETCOREAPP
            await filter.OnResultExecutionAsync(executingContext, () => Task.FromResult(new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), executingContext.Result, foosController)));
#else
            await filter.OnActionExecutedAsync(contextMock, new CancellationToken());
#endif
        }
        
        [TestMethod]
        public async Task NullValueFromControllerIsHandled()
        {
            // Mock a translator
            var testServiceMock = new Mock<ITranslatorService>();
            testServiceMock
                .Setup(service => service.TranslateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ITranslatorService should not be called for null values"));

#if NETCOREAPP
            var foosController = new FoosController();
            var controllerActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = foosController.GetType().GetMethod(nameof(foosController.ReadNullAsync))
            };
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = controllerActionDescriptor
            };
            var executingContext = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(null), foosController);
#else
            var contextMock = CreateExecutedContextWithStatusCode(HttpStatusCode.OK, null);
#endif

            // Setup the filter
            var filter = new ValueTranslatorFilter(testServiceMock.Object, () => Foo.ConsumerName);

            // Run the filter
            // There are no translations, so ITranslatorService.TranslateAsync should not be called
#if NETCOREAPP
            await filter.OnResultExecutionAsync(executingContext, () => Task.FromResult(new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), executingContext.Result, foosController)));
#else
            await filter.OnActionExecutedAsync(contextMock, new CancellationToken());
#endif
        }

#if NETCOREAPP
#else
        private HttpActionContext CreateExecutingContext(Dictionary<string, object> actionArguments, string methodName)
        {
            var routes = new HttpRouteCollection();
            var methodInfo = typeof(FoosController).GetMethod(methodName);
            var context = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(),
                    Controller = new FoosController()
                },
                ActionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(new HttpConfiguration(routes), nameof(FoosController), typeof(FoosController)), methodInfo)
            };
            foreach (var entry in actionArguments)
            {
                context.ActionArguments.Add(entry.Key, entry.Value);
            }
            return context;
        }

        private HttpActionExecutedContext CreateExecutedContextWithStatusCode(HttpStatusCode statusCode, object model)
        {
            return new HttpActionExecutedContext
            {
                ActionContext = new HttpActionContext
                {
                    ControllerContext = new HttpControllerContext
                    {
                        Request = new HttpRequestMessage()
                    }
                },
                Response = new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = model == null ? null : new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
                }
            };
        }
#endif
    }
}