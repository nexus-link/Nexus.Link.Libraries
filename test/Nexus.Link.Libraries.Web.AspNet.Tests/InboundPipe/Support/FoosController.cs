
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
    internal class FoosController : ControllerBase, IRead<Foo, string>, IUpdateAndReturn<Foo, string>
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
        public Task<Foo> UpdateAndReturnAsync([TranslationConcept("foo.id")]string id, Foo item,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            Assert.IsTrue(item.Id.StartsWith("(foo.id!"));
            InternalContract.Require(id == item.Id, $"Expected {nameof(id)} to be identical to {nameof(item)}.{nameof(item.Id)}.");
            return Task.FromResult(item);
        }
    }
}
#endif
