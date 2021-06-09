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

        [JsonConverter(typeof(NodeConverter))] public Node Node { get; }

        [JsonConverter(typeof(AnimationPathConverter))]
        public AnimationPath Path { get; }
    }
}