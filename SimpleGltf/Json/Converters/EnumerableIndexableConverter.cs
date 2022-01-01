using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters;

internal class EnumerableIndexableConverter<T> : JsonConverter<IEnumerable<T>> where T : IIndexable
{
    public override IList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<T> values, JsonSerializerOptions options)
    {
        if (values == null)
            return;
        writer.WriteStartArray();
        foreach (var value in values)
            writer.WriteNumberValue(value.Index);
        writer.WriteEndArray();
    }
}