using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Texture
    {
        internal readonly int Index;

        internal Texture(GltfAsset gltfAsset, Sampler sampler, Image image, string name)
        {
            gltfAsset.Textures ??= new List<Texture>();
            Index = gltfAsset.Textures.Count;
            gltfAsset.Textures.Add(this);
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