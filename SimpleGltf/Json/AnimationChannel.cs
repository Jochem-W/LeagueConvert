using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationChannel
    {
        internal AnimationChannel(Animation animation, AnimationSampler sampler, AnimationTarget target)
        {
            animation.Channels.Add(this);
            Sampler = sampler;
            Target = target;
        }

        [JsonConverter(typeof(IndexableConverter<AnimationSampler>))] public AnimationSampler Sampler { get; }

        public AnimationTarget Target { get; }
    }
}