using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationTarget
    {
        public AnimationTarget(AnimationPath path)
        {
            Path = path;
        }

        [JsonConverter(typeof(IndexableConverter<Node>))]
        public Node Node { get; init; }

        [JsonConverter(typeof(AnimationPathConverter))]
        public AnimationPath Path { get; }
    }
}