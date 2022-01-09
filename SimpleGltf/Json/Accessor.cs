using System.Collections;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class Accessor : IIndexable
{
    internal int ActualByteOffset;

    internal Accessor(GltfAsset gltfAsset, BufferView bufferView, AccessorType type)
    {
        Index = gltfAsset.AccessorList.Count;
        gltfAsset.AccessorList.Add(this);
        BufferView = bufferView;
        BufferView.Accessors.Add(this);
        Type = type;
    }

    [JsonConverter(typeof(IndexableConverter<BufferView>))]
    public BufferView BufferView { get; }

    public int? ByteOffset => ActualByteOffset != 0 ? ActualByteOffset : null;

    public ComponentType ComponentType { get; protected init; }

    public int Count { get; set; }

    [JsonConverter(typeof(AccessorTypeConverter))]
    public AccessorType Type { get; }

    public IEnumerable Max { get; internal set; }

    public IEnumerable Min { get; set; }

    [JsonIgnore] public int Index { get; }
}