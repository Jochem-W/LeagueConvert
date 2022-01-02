using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Extensions;

public class ExtensionsConverter : JsonConverter<IEnumerable<KeyValuePair<string, IDictionary>>>
{
    public override IEnumerable<KeyValuePair<string, IDictionary>> Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, IDictionary>> pairs,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var (name, _) in pairs)
        {
            writer.WritePropertyName(name);
            writer.WriteStartObject();
            //TODO
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}