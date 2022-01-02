using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters;

internal class DictionaryIndexableConverter<T> : JsonConverter<IEnumerable<KeyValuePair<string, T>>>
    where T : IIndexable
{
    public override IEnumerable<KeyValuePair<string, T>> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, T>> pairs,
        JsonSerializerOptions options)
    {
        if (pairs == null)
            return;
        writer.WriteStartObject();
        foreach (var (name, value) in pairs)
            writer.WriteNumber(name, value.Index);
        writer.WriteEndObject();
    }
}