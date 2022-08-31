using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;

internal interface IChildService<T> : IReadChildren<T, string>
{
    
}