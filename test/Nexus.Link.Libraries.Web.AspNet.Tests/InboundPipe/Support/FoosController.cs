
using System;
#if NETCOREAPP
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    [ApiController]
    [Route("api/Foos")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class FoosController : ControllerBase, IRead<Foo, string>, IUpdateAndReturn<Foo, string>
    {
        /// <inheritdoc />
        [HttpGet("{id}")]
        public Task<Foo> ReadAsync(string id, CancellationToken token = default(CancellationToken))
        {
            var item = new Foo {Id = id, Name = "name"};
            return Task.FromResult(item);
        }

        /// <inheritdoc />
        [HttpPut("{id}")]
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
#endif
