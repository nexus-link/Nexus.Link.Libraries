using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Translation
{
    /// <summary>
    /// What a translator service need to fulfill.
    /// </summary>
    public interface ITranslatorService
    {
        /// <summary>
        /// Go through the <paramref name="conceptValuePaths"/> and return a dictionary with translated values..
        /// </summary>
        /// <param name="conceptValuePaths">The values that needs to be translated.</param>
        /// <param name="targetClientName">The client that we should translate to.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A dictionary with concept values as keys and the translated values as values.</returns>
        Task<IDictionary<string, string>> TranslateAsync(IEnumerable<string> conceptValuePaths, string targetClientName, CancellationToken cancellationToken = default(CancellationToken));
    }
}