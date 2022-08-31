using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;

internal class TreeService : ResourceService<Tree>
{
    public TreeService(IHttpSender httpSender) : base(httpSender.CreateHttpSender("trees"), "tree")
    {
    }
}