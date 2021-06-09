using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class MaterialConverter : JsonConverter<Material>
    {
        public override Material Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Material value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Materials.IndexOf(value));
        }
    }
}