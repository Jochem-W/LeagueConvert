using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class TextureConverter : JsonConverter<Texture>
    {
        public override Texture Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Texture value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Textures.IndexOf(value));
        }
    }
}