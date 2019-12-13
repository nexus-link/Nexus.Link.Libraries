
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Interfaces;

#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
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

        /// <inheritdoc />
#if NETCOREAPP
        [HttpPut("{id}")]
#else
        [HttpGet]
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
    }
}
