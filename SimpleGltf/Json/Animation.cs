using System.Collections.Generic;

namespace SimpleGltf.Json
{
    public class Animation
    {
        internal Animation(GltfAsset gltfAsset, string name)
        {
            gltfAsset.Animations ??= new List<Animation>();
            gltfAsset.Animations.Add(this);
            Channels = new List<Channel>();
            Samplers = new List<AnimationSampler>();
            Name = name;
        }

        public IList<Channel> Channels { get; }

        public IList<AnimationSampler> Samplers { get; }

        public string Name { get; }
    }
}