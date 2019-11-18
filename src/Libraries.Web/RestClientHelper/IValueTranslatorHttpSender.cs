using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

#pragma warning disable 1591

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    public interface IValueTranslatorHttpSender : IHttpSender
    {
        Task<HttpOperationResponse<string>> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            string resultConceptName,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
