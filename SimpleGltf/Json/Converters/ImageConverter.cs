using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class ImageConverter : JsonConverter<Image>
    {
        public override Image Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Image value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Images.IndexOf(value));
        }
    }
}