using System.Linq;
using System.Text.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Extensions;

internal static class JsonExtensions
{
    public static JsonElement? SafeGet(this JsonElement inJsonElement, string propertyName, bool optional)
    {
        var success = inJsonElement.TryGetProperty(propertyName, out var outElement);
        if (!success)
        {
            InternalContract.Require(success || optional, $"The parameter {nameof(propertyName)} ({propertyName}) did not exist in {nameof(inJsonElement)}.");
            return null;
        }
        return outElement;
    }

    public static T? SafeGet<T>(this JsonElement inJsonElement, string propertyName, bool optional)
    {
        var outJsonElement = inJsonElement.SafeGet(propertyName, true);
        InternalContract.Require(outJsonElement != null || optional, $"The parameter {nameof(propertyName)} ({propertyName}) did not exist in {nameof(inJsonElement)}.");
        if (outJsonElement == null) return default;
        JsonElement jsonElement = (JsonElement)outJsonElement;
        var type = typeof(T);
        if (type.Name == "Nullable`1")
        {
            type = type.GenericTypeArguments.First();
        }
        var typeName = type.Name;
        switch (typeName)
        {
            case "String":
                return (T)(object)jsonElement.ToString();
            case "Boolean":
                return (T)(object)jsonElement.GetBoolean();
            case "JsonElement":
                return (T)(object)jsonElement;
        }

        FulcrumAssert.Fail(CodeLocation.AsString());
        return default!;
    }
    public static JsonElement SafeGet(this JsonElement inJsonElement, string propertyName)
    {
        var outJsonElement = SafeGet(inJsonElement, propertyName, false);
        FulcrumAssert.IsNotNull(outJsonElement, CodeLocation.AsString());
        return (JsonElement) outJsonElement!;
    }

    public static T SafeGet<T>(this JsonElement inJsonElement, string propertyName)
    {
        var result = SafeGet<T>(inJsonElement, propertyName, false);
        FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
        return result!;
    }

}