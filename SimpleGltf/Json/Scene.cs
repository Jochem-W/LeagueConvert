using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class Scene : IIndexable
{
    internal readonly IList<Node> NodeList = new List<Node>();

    internal Scene(GltfAsset gltfAsset)
    {
        Index = gltfAsset.SceneList.Count;
        gltfAsset.SceneList.Add(this);
    }

    [JsonConverter(typeof(EnumerableIndexableConverter<Node>))]
    public IEnumerable<Node> Nodes => NodeList.Count > 0 ? NodeList : null;

    [JsonIgnore] public int Index { get; }
}