using System.Text.Json;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Extensions;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;

public class Resource : IUniquelyIdentifiable<string>, IConvertibleFromJsonElement
{
    /// <inheritdoc />
    [Validation.NotNullOrWhitespace]
    public string Id { get; set; } = null!;

    [Validation.NotNullOrWhitespace]
    public string Name { get; set; } = null!;

    /// <inheritdoc />
    public virtual void From(JsonElement jsonElement)
    {
        Id = jsonElement.SafeGet<string>("objectId", true) ?? jsonElement.SafeGet<string>("id");
        Name = jsonElement.SafeGet<string>("name", true) ?? jsonElement.SafeGet<string>("path", true) ?? jsonElement.SafeGet<string>("relativePath");
    }
}