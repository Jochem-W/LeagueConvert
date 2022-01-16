using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class AnimationChannel
{
    internal AnimationChannel(Animation animation, AnimationSampler sampler,
        AnimationChannelTarget animationChannelTarget)
    {
        animation.ChannelList.Add(this);
        Sampler = sampler;
        AnimationChannelTarget = animationChannelTarget;
    }

    [JsonConverter(typeof(IndexableConverter<AnimationSampler>))]
    public AnimationSampler Sampler { get; }

    public AnimationChannelTarget AnimationChannelTarget { get; }
}