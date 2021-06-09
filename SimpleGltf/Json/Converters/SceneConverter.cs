using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class SceneConverter : JsonConverter<Scene>
    {
        public override Scene Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Scene value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Scenes.IndexOf(value));
        }
    }
}