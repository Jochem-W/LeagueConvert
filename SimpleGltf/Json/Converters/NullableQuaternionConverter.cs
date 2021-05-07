using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class NullableQuaternionConverter : JsonConverter<Quaternion?>
    {
        public override Quaternion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Quaternion? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
                return;
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Value.X);
            writer.WriteNumberValue(value.Value.Y);
            writer.WriteNumberValue(value.Value.Z);
            writer.WriteNumberValue(value.Value.W);
            writer.WriteEndArray();
        }
    }
}