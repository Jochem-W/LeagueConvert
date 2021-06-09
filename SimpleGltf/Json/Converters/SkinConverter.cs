using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class SkinConverter : JsonConverter<Skin>
    {
        public override Skin Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Skin value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Skins.IndexOf(value));
        }
    }
}