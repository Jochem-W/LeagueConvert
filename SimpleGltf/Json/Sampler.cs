using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json;

public class Sampler : IIndexable
{
    internal Sampler(GltfAsset gltfAsset)
    {
        Index = gltfAsset.SamplerList.Count;
        gltfAsset.SamplerList.Add(this);
    }

    public WrappingMode? WrapS { get; init; }

    public WrappingMode? WrapT { get; init; }

    [JsonIgnore] public int Index { get; }
}