using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;

public class ProjectService : ResourceService<Resource>
{
    public ProjectService(IHttpSender httpSender) : base(httpSender.CreateHttpSender("_apis/projects"), "project")
    {
    }
}