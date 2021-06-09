using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Skin
    {
        internal readonly GltfAsset GltfAsset;

        internal Skin(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Skins ??= new List<Skin>();
            GltfAsset.Skins.Add(this);
            Joints = new List<Node>();
            Name = name;
        }

        [JsonConverter(typeof(AccessorConverter))]
        public Accessor InverseBindMatrices { get; set; }

        [JsonConverter(typeof(NodeConverter))] public Node Skeleton { get; }

        [JsonConverter(typeof(NodeListConverter))]
        public IList<Node> Joints { get; }

        public string Name { get; set; }
    }
}