using System.Text.Json.Serialization;

namespace SimpleGltf.Json;

public class Mesh : IIndexable
{
    internal readonly IList<MeshPrimitive> PrimitiveList = new List<MeshPrimitive>();

    internal Mesh(GltfAsset gltfAsset)
    {
        Index = gltfAsset.MeshList.Count;
        gltfAsset.MeshList.Add(this);
    }

    public IEnumerable<MeshPrimitive> Primitives => PrimitiveList;

    [JsonIgnore] public int Index { get; }
}