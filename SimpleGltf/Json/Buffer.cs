using System.Text.Json.Serialization;

namespace SimpleGltf.Json;

public class Buffer : IIndexable
{
    internal readonly IList<BufferView> BufferViews = new List<BufferView>();
    internal Stream Stream = null;

    internal Buffer(GltfAsset gltfAsset)
    {
        Index = gltfAsset.BufferList.Count;
        gltfAsset.BufferList.Add(this);
    }

    public string Uri { get; internal set; }

    public int ByteLength => (int)Stream.Length;

    [JsonIgnore] public int Index { get; internal set; }
}