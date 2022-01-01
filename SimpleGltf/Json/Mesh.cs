using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json;

public class Mesh : IIndexable
{
    internal readonly IList<Primitive> PrimitiveList = new List<Primitive>();

    internal Mesh(GltfAsset gltfAsset)
    {
        Index = gltfAsset.MeshList.Count;
        gltfAsset.MeshList.Add(this);
    }

    public IEnumerable<Primitive> Primitives => PrimitiveList;

    [JsonIgnore] public int Index { get; }
}