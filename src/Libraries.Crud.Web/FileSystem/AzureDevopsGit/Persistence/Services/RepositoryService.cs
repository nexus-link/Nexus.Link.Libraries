using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;

internal class RepositoryService : ResourceService<Resource>
{
    public RepositoryService(IHttpSender httpSender, string? projectId = null) : base(httpSender.CreateHttpSender("repositories"), "repository")
    {
        if (projectId != null)
        {
            HttpSender = httpSender.CreateHttpSender($"{projectId}/_apis/git/repositories");
        }
    }
}