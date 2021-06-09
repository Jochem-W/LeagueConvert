using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class BufferConverter : JsonConverter<Buffer>
    {
        public override Buffer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Buffer value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Buffers.IndexOf(value));
        }
    }
}