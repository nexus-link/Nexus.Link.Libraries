
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Interfaces;

#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
#if NETCOREAPP
    [ApiController]
    [Route("api/Foos")]
#else
    [RoutePrefix("api/Foos")]
#endif
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class FoosController :
#if NETCOREAPP
        ControllerBase
#else
        ApiController
#endif
        , IRead<Foo, string>, IUpdateAndReturn<Foo, string>
    {
        /// <inheritdoc />
#if NETCOREAPP
        [HttpGet("{id}")]
#else
        [HttpGet]
        [Route("{id}")]
#endif
        public Task<Foo> ReadAsync(string id, CancellationToken token = default(CancellationToken))
        {
            var item = new Foo {Id = id, Name = "name"};
            return Task.FromResult(item);
        }

#if NETCOREAPP
        [HttpGet("NullBars")]
#else
        [HttpGet]
        [Route("NullBars")]
#endif
        public Task<Foo> ReadNullAsync(CancellationToken token = default(CancellationToken))
        {
            return null;
        }

        /// <inheritdoc />
#if NETCOREAPP
        [HttpPut("{id}")]
#else
        [HttpPut]
#endif
        [Route("{id}")]
        public Task<Foo> UpdateAndReturnAsync([TranslationConcept(Foo.IdConceptName)]string id, Foo item,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            var success = ConceptValue.TryParse(item.Id, out var conceptValue);
            FulcrumAssert.IsTrue(success);
            FulcrumAssert.AreEqual(Foo.IdConceptName, conceptValue.ConceptName);
            FulcrumAssert.AreEqual(Foo.ConsumerName, conceptValue.ClientName);
            InternalContract.Require(id == item.Id, $"Expected {nameof(id)} to be identical to {nameof(item)}.{nameof(item.Id)}.");
            item.Id = Translator.Decorate(Foo.IdConceptName, Foo.ProducerName, Foo.ProducerId1);
            return Task.FromResult(item);
        }

        /// <summary>
        /// An endpoint without translations
        /// </summary>
        /// <returns></returns>
#if NETCOREAPP
        [HttpGet("ServiceTenant")]
#else
        [HttpPut]
#endif
        [Route("ServiceTenant")]
        public Task<Tenant> ServiceTenant()
        {
            return Task.FromResult(FulcrumApplication.Setup.Tenant);
        }

#if NETCOREAPP
        [HttpGet("~/api/CurrentNexusTestContextValue")]
        [Produces("application/json")]
#else
        [HttpGet]
        [Route("~/api/CurrentNexusTestContextValue")]
#endif
        public string CurrentNexusTestContextValue()
        {
            // Setup by DelegatingHandler registered in TestStartup
            return FulcrumApplication.Context.NexusTestContext ?? "<not setup>";
        }
    }
}
