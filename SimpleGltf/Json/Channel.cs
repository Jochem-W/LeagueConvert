using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class Channel
{
    internal Channel(Animation animation, AnimationSampler sampler, Target target)
    {
        animation.ChannelList.Add(this);
        Sampler = sampler;
        Target = target;
    }

    [JsonConverter(typeof(IndexableConverter<AnimationSampler>))]
    public AnimationSampler Sampler { get; }

    public Target Target { get; }
}