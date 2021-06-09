using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class MeshConverter : JsonConverter<Mesh>
    {
        public override Mesh Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Mesh value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Meshes.IndexOf(value));
        }
    }
}