using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Skin
    {
        private readonly GltfAsset _gltfAsset;

        internal Skin(GltfAsset gltfAsset, string name)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Skins ??= new List<Skin>();
            _gltfAsset.Skins.Add(this);
            Joints = new List<Node>();
            Name = name;
        }

        [JsonIgnore] public Accessor InverseBindMatrices { get; set; }

        [JsonPropertyName("inverseBindMatrices")]
        public int? InverseBindMatricesReference =>
            InverseBindMatrices == null ? null : _gltfAsset.Accessors.IndexOf(InverseBindMatrices);

        [JsonIgnore] public Node Skeleton { get; set; }

        [JsonPropertyName("skeleton")]
        public int? SkeletonReference => Skeleton == null ? null : _gltfAsset.Nodes.IndexOf(Skeleton);

        [JsonIgnore] public IList<Node> Joints { get; }

        [JsonPropertyName("joints")]
        public IEnumerable<int> JointReferences => Joints.Select(joint => _gltfAsset.Nodes.IndexOf(joint));

        public string Name { get; set; }
    }
}