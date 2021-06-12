using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Texture : IIndexable
    {
        internal Texture(GltfAsset gltfAsset)
        {
            gltfAsset.Textures ??= new List<Texture>();
            Index = gltfAsset.Textures.Count;
            gltfAsset.Textures.Add(this);
        }
        
        [JsonIgnore] public int Index { get; }

        [JsonConverter(typeof(IndexableConverter<Sampler>))] public Sampler Sampler { get; init; }

        [JsonConverter(typeof(IndexableConverter<Image>))] public Image Source { get; init; }
    }
}