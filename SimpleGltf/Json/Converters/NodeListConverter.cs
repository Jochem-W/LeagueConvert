using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class NodeListConverter : JsonConverter<IList<Node>>
    {
        public override IList<Node> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IList<Node> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var node in value)
                writer.WriteNumberValue(node.GltfAsset.Nodes.IndexOf(node));
            writer.WriteEndArray();
        }
    }
}