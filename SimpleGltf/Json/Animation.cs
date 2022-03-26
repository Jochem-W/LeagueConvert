namespace SimpleGltf.Json;

public class Animation
{
    internal readonly IList<AnimationChannel> ChannelList = new List<AnimationChannel>();
    internal readonly IList<AnimationSampler> SamplerList = new List<AnimationSampler>();

    internal Animation(GltfAsset gltfAsset)
    {
        gltfAsset.AnimationList.Add(this);
    }

    public IEnumerable<AnimationChannel> Channels => ChannelList;

    public IEnumerable<AnimationSampler> Samplers => SamplerList;

    public string Name { get; set; }
}