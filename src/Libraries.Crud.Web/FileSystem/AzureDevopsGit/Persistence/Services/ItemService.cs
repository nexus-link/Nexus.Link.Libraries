using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;

public class ItemService : ResourceService<Resource>
{
    public ItemService(IHttpSender httpSender) : base(httpSender.CreateHttpSender("items"), "item")
    {
    }
}