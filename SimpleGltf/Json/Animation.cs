using System.Collections.Generic;

namespace SimpleGltf.Json;

public class Animation
{
    internal readonly IList<Channel> ChannelList = new List<Channel>();
    internal readonly IList<AnimationSampler> SamplerList = new List<AnimationSampler>();

    internal Animation(GltfAsset gltfAsset)
    {
        gltfAsset.AnimationList.Add(this);
    }

    public IEnumerable<Channel> Channels => ChannelList;

    public IEnumerable<AnimationSampler> Samplers => SamplerList;

    public string Name { get; init; }
}