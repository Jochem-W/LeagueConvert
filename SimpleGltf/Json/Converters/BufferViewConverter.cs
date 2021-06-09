using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class BufferViewConverter : JsonConverter<BufferView>
    {
        public override BufferView Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, BufferView value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.BufferViews.IndexOf(value));
        }
    }
}