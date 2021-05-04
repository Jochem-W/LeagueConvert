using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Texture
    {
        private readonly Image _image;
        private readonly Sampler _sampler;

        internal readonly GltfAsset GltfAsset;

        internal Texture(GltfAsset gltfAsset, Sampler sampler, Image image, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Textures ??= new List<Texture>();
            GltfAsset.Textures.Add(this);
            _sampler = sampler;
            _image = image;
            Name = name;
        }

        [JsonPropertyName("sampler")] public int? SamplerReference => GltfAsset.Samplers.IndexOf(_sampler);

        [JsonPropertyName("source")] public int? ImageReference => GltfAsset.Images.IndexOf(_image);

        public string Name { get; }
    }
}