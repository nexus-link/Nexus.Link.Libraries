using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Crud.ServerTranslators.From
{
    /// <summary>
    /// A fake class for obsolete translation classes.
    /// </summary>
    public class FakeTranslatorService : ITranslatorService
    {
        /// <inheritdoc />
        public Task<IDictionary<string, string>> TranslateAsync(IEnumerable<string> conceptValuePaths, string targetClientName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            FulcrumAssert.Fail($"The method(s) of class {nameof(FakeTranslatorService)} is not expected to be called.");
            return Task.FromResult((IDictionary<string, string>)null);
        }
    }
}
