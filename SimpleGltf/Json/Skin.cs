using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Skin
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;

        internal Skin(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Skins ??= new List<Skin>();
            Index = GltfAsset.Skins.Count;
            GltfAsset.Skins.Add(this);
            Joints = new List<Node>();
            Name = name;
        }

        [JsonIgnore] public IAccessor InverseBindMatrices { get; set; }

        [JsonPropertyName("inverseBindMatrices")]
        public int InverseBindMatricesIndex => InverseBindMatrices.Index;

        [JsonIgnore] public Node Skeleton { get; }

        [JsonPropertyName("skeleton")] public int? SkeletonIndex => Skeleton?.Index;

        [JsonIgnore] public IList<Node> Joints { get; }

        [JsonPropertyName("joints")] public IEnumerable<int> JointsIndices => Joints.Select(joint => joint.Index);

        public string Name { get; set; }
    }
}