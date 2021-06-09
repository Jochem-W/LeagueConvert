using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class NodeConverter : JsonConverter<Node>
    {
        public override Node Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Node value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Nodes.IndexOf(value));
        }
    }
}