using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public interface IAccessor
{
    [JsonConverter(typeof(IndexableConverter<BufferView>))]
    public BufferView BufferView { get; }

    public int? ByteOffset { get; }

    public ComponentType ComponentType { get; }

    public int Count { get; }

    [JsonConverter(typeof(AccessorTypeConverter))]
    public AccessorType Type { get; }

    [JsonIgnore] public int Index { get; }
}