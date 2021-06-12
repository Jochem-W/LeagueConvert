using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Skin : IIndexable
    {
        internal Skin(GltfAsset gltfAsset)
        {
            gltfAsset.Skins ??= new List<Skin>();
            Index = gltfAsset.Skins.Count;
            gltfAsset.Skins.Add(this);
            Joints = new List<Node>();
        }
        
        [JsonIgnore] public int Index { get; }

        [JsonConverter(typeof(IndexableConverter<IAccessor>))] public IAccessor InverseBindMatrices { get; set; }

        [JsonConverter(typeof(EnumerableIndexableConverter<Node>))] public IList<Node> Joints { get; }
    }
}