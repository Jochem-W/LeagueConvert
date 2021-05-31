using System.Collections.Generic;

namespace SimpleGltf.Json
{
    public class Animation
    {
        private readonly GltfAsset _gltfAsset;

        internal Animation(GltfAsset gltfAsset, string name)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Animations ??= new List<Animation>();
            _gltfAsset.Animations.Add(this);
            Channels = new List<Channel>();
            Samplers = new List<AnimationSampler>();
            Name = name;
        }

        public IList<Channel> Channels { get; }

        public IList<AnimationSampler> Samplers { get; }

        public string Name { get; }
    }
}