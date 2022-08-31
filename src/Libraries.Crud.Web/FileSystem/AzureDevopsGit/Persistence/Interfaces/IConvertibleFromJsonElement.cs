using System.Text.Json;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;

internal interface IConvertibleFromJsonElement
{
    void From(JsonElement jsonElement);
}