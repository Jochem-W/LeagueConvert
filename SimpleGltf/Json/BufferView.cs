using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class BufferView : IIndexable
{
    internal readonly IList<Accessor> Accessors = new List<Accessor>();
    internal readonly BinaryWriter BinaryWriter = new(new MemoryStream());
    internal int ActualByteOffset;
    internal int ActualByteStride = 0;
    internal bool Stride = true;

    internal BufferView(GltfAsset gltfAsset, Buffer buffer)
    {
        Index = gltfAsset.BufferViewList.Count;
        gltfAsset.BufferViewList.Add(this);
        Buffer = buffer;
        Buffer.BufferViews.Add(this);
    }

    [JsonConverter(typeof(IndexableConverter<Buffer>))]
    public Buffer Buffer { get; }

    public int? ByteOffset => ActualByteOffset != 0 ? ActualByteOffset : null;

    public int ByteLength => (int) BinaryWriter.BaseStream.Length;

    public int? ByteStride => ActualByteStride != 0 ? ActualByteStride : null;

    public BufferViewTarget? Target { get; init; }

    [JsonIgnore] public int Index { get; internal set; }

    public void StopStride()
    {
        Stride = false;
    }
}