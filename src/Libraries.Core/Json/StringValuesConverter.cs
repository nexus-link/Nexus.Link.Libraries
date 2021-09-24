using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Json
{
    /// <summary>
    /// Extension methods for <see cref="JsonConverter"/>
    /// </summary>
    public static class JsonConverterCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="StringValues"/> JSON converter to the list.
        /// </summary>
        /// <param name="converters">The <see cref="T:ICollection{JsonConverter}" /> to extend.</param>
        /// <returns>A reference to <paramref name="converters"/> after the operation has completed.</returns>
        public static ICollection<JsonConverter> AddStringValuesConverter(this ICollection<JsonConverter> converters)
        {
            converters.Add(new StringValuesConverter());
            return converters;
        }
    }

    /// <summary>
    /// A custom <see cref="JsonConverter"/> for <see cref="StringValues"/>.
    /// </summary>
    public class StringValuesConverter : JsonConverter<StringValues>
    {
        /// <inheritdoc />
        public StringValuesConverter()
        {
            CanWrite = false;
        }

        /// <inheritdoc />
        public override bool CanWrite { get; }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, StringValues value, JsonSerializer serializer)
        {
            //Not needed because CanWrite is false
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public override StringValues ReadJson(JsonReader reader, Type objectType, StringValues existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var currentValues = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.EndArray)
                {
                    currentValues.Add(reader.Value.ToString());
                }
                else
                {
                    return new StringValues(currentValues.ToArray());
                }
            }

            return existingValue;
        }
    }
}