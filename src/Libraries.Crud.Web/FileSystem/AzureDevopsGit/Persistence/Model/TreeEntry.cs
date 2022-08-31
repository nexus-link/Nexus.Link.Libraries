using System.Text.Json;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Extensions;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;

internal class TreeEntry : Resource, IConvertibleFromJsonElement
{
    [Validation.NotNullOrWhitespace]
    public GitObjectTypeEnum GitObjectType { get; private set; }

    /// <inheritdoc />
    public override void From(JsonElement jsonElement)
    {
        base.From(jsonElement);
        var gitObjectType = jsonElement.SafeGet<string>("gitObjectType");
        GitObjectType = gitObjectType.ToEnum<GitObjectTypeEnum>();
    }
}
public enum GitObjectTypeEnum
{
    Tree,
    Blob
}