using System.Collections;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public interface IAccessor : IIndexable
    {
        [JsonIgnore] GltfAsset GltfAsset { get; }

        [JsonIgnore] BufferView BufferView { get; }

        [JsonPropertyName("bufferView")] int BufferViewIndex => BufferView.Index;

        int? ByteOffset { get; }

        ComponentType ComponentType { get; }

        bool? Normalized { get; }

        int Count { get; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        AccessorType Type { get; }

        IEnumerable Max { get; }

        IEnumerable Min { get; }

        string Name { get; }
    }
}