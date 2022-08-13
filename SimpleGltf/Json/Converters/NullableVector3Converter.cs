using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters;

internal class NullableVector3Converter : JsonConverter<Vector3?>
{
    public override Vector3? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Vector3? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            return;
        }

        writer.WriteStartArray();
        writer.WriteNumberValue(value.Value.X);
        writer.WriteNumberValue(value.Value.Y);
        writer.WriteNumberValue(value.Value.Z);
        writer.WriteEndArray();
    }
}