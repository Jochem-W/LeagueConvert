using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Target
    {
        private readonly Node _node;

        public Target(AnimationPath path)
        {
            Path = path;
        }

        [JsonConverter(typeof(IndexableConverter<Node>))]
        public Node Node
        {
            get => _node;
            init
            {
                _node = value;
                _node.Animated = true;
            }
        }

        [JsonConverter(typeof(AnimationPathConverter))]
        public AnimationPath Path { get; }
    }
}