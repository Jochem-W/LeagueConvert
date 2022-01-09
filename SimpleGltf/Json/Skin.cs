using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class Skin : IIndexable
{
    internal readonly IList<Node> JointList = new List<Node>();

    internal Skin(GltfAsset gltfAsset)
    {
        Index = gltfAsset.SkinList.Count;
        gltfAsset.SkinList.Add(this);
    }

    [JsonConverter(typeof(IndexableConverter<Accessor>))]
    public Accessor InverseBindMatrices { get; set; }

    [JsonConverter(typeof(EnumerableIndexableConverter<Node>))]
    public IEnumerable<Node> Joints => JointList;

    [JsonIgnore] public int Index { get; }
}