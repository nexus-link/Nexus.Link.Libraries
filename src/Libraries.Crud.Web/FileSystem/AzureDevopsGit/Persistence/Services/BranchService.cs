using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;

internal class BranchService : ResourceService<Resource>, IChildService<Resource>
{
    public BranchService(IHttpSender httpSender, string? repositoryId = null) : base(httpSender.CreateHttpSender("refs"), "branch")
    {
        if (repositoryId != null)
        {
            HttpSender = httpSender.CreateHttpSender($"_apis/git/repositories/{repositoryId}/refs");
        }
    }
}