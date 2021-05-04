using System.Collections.Generic;

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

        public int? SamplerReference => GltfAsset.Samplers.IndexOf(_sampler);

        public int? ImageReference => GltfAsset.Images.IndexOf(_image);

        public string Name { get; }
    }
}