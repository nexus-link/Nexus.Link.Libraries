using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Extensions;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Interfaces;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;

internal class ResourceService<T> : IParentService<T>, IChildService<T>, IRead<T, string>
where T : IConvertibleFromJsonElement, new()
{
    private readonly string _name;
    private readonly Dictionary<string, List<string>>? _headers = null;
    protected const string ApiVersion = "6.0";
    protected IHttpSender HttpSender { get; set; }

    public ResourceService(IHttpSender httpSender, string name)
    {
        _name = name;
        InternalContract.RequireNotNull(httpSender, CodeLocation.AsString());
        HttpSender = httpSender;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> ReadAllAsync(int limit = 2147483647, CancellationToken cancellationToken = new CancellationToken())
    {
        var response = await HttpSender.SendRequestAsync(HttpMethod.Get, $"?api-version={ApiVersion}", _headers, cancellationToken);
        var j1 = await GetJsonAsync($"Read all {_name}", response, cancellationToken);
        var j2 = j1.SafeGet("value");
        return j2.EnumerateArray().Select(ToItem);
    }

    private T ToItem(JsonElement jsonElement)
    {
        var item = new T();
        item.From(jsonElement);
        FulcrumAssert.IsValidated(item, CodeLocation.AsString());
        return item;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> ReadChildrenAsync(string parentId, int limit = 2147483647,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var response = await HttpSender.SendRequestAsync(HttpMethod.Get, $"{parentId}?api-version={ApiVersion}", null, cancellationToken);
        var jsonElement = await GetJsonAsync($"Read all {_name} ({parentId}) children", response, cancellationToken);
        var valueJsonElement = jsonElement.SafeGet("value", true);
        jsonElement = valueJsonElement ?? jsonElement;
        return jsonElement.EnumerateArray().Select(ToItem);
    }

    /// <inheritdoc />
    public async Task<T> ReadAsync(string id, CancellationToken cancellationToken = new CancellationToken())
    {
        var response = await HttpSender.SendRequestAsync(HttpMethod.Get, $"{id}?api-version={ApiVersion}", null, cancellationToken);
        var jsonElement = await GetJsonAsync($"Read {_name} ({id})", response, cancellationToken);
        return ToItem(jsonElement);
    }

    private static async Task<JsonElement> GetJsonAsync(string title, HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(response, nameof(response));
        var responseContent = await Nexus.Link.Libraries.Web.RestClientHelper.HttpSender.TryGetContentAsStringAsync(response.Content, false, cancellationToken);
        try
        {
            var json = JsonDocument.Parse(responseContent!);
            return json.RootElement;
        }
        catch (Exception e)
        {
            throw new FulcrumResourceException(
                $"The response to request {title} could not parsed into JSON. The content was:\r{responseContent}.",
                e);
        }
    }
}