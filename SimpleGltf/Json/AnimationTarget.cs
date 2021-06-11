using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationTarget
    {
        public AnimationTarget(Node node, AnimationPath path)
        {
            Node = node;
            Path = path;
        }

        [JsonIgnore] public Node Node { get; }

        [JsonPropertyName("node")] public int NodeIndex => Node.Index;

        [JsonConverter(typeof(AnimationPathConverter))]
        public AnimationPath Path { get; }
    }
}