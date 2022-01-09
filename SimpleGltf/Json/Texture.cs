using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class Texture : IIndexable
{
    internal Texture(GltfAsset gltfAsset)
    {
        Index = gltfAsset.TextureList.Count;
        gltfAsset.TextureList.Add(this);
    }

    [JsonConverter(typeof(IndexableConverter<Sampler>))]
    public Sampler Sampler { get; init; }

    [JsonConverter(typeof(IndexableConverter<Image>))]
    public Image Source { get; init; }

    [JsonIgnore] public int Index { get; }
}