using System.Collections.Generic;
using System.Text.Json;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Extensions;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;

public class Tree : IUniquelyIdentifiable<string>, IConvertibleFromJsonElement
{
    /// <inheritdoc />
    [Validation.NotNullOrWhitespace]
    public string Id { get; set; } = null!;

    [Validation.NotNull]
    public IList<TreeEntry> TreeEntries { get; } = new List<TreeEntry>();

    /// <inheritdoc />
    public void From(JsonElement jsonElement)
    {
        Id = jsonElement.SafeGet<string>("objectId", true) ?? jsonElement.SafeGet<string>("id");
        var entriesAsJson = jsonElement.SafeGet<JsonElement>("treeEntries");
        foreach (var entryAsJson in entriesAsJson.EnumerateArray())
        {
            var entry = new TreeEntry();
            entry.From(entryAsJson);
            TreeEntries.Add(entry);
        }
    }
}