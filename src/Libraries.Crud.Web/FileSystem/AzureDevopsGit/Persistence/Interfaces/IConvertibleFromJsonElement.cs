using System.Text.Json;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;

public interface IConvertibleFromJsonElement
{
    void From(JsonElement jsonElement);
}