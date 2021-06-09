using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Texture
    {
        internal readonly GltfAsset GltfAsset;

        internal Texture(GltfAsset gltfAsset, Sampler sampler, Image image, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Textures ??= new List<Texture>();
            GltfAsset.Textures.Add(this);
            Sampler = sampler;
            Source = image;
            Name = name;
        }

        [JsonConverter(typeof(SamplerConverter))]
        public Sampler Sampler { get; }

        [JsonConverter(typeof(ImageConverter))]
        public Image Source { get; }

        public string Name { get; }
    }
}