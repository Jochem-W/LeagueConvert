using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Texture
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;

        internal Texture(GltfAsset gltfAsset, Sampler sampler, Image image, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Textures ??= new List<Texture>();
            Index = GltfAsset.Textures.Count;
            GltfAsset.Textures.Add(this);
            Sampler = sampler;
            Source = image;
            Name = name;
        }

        [JsonIgnore] public Sampler Sampler { get; }

        [JsonPropertyName("sampler")] public int SamplerIndex => Sampler.Index;

        [JsonIgnore] public Image Source { get; }

        [JsonPropertyName("source")] public int SourceIndex => Source.Index;

        public string Name { get; }
    }
}