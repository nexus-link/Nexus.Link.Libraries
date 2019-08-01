using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nexus.Link.Services.Controllers.Capabilities.Api
{
    public interface IRootService
    {
        Task<ContentResult> Welcome(CancellationToken token = default(CancellationToken));
    }
}