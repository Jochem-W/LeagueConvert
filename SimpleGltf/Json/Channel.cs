using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Channel
    {
        private readonly Animation _animation;

        internal Channel(Animation animation, AnimationSampler sampler, AnimationTarget target)
        {
            _animation = animation;
            _animation.Channels.Add(this);
            Sampler = sampler;
            Target = target;
        }

        [JsonConverter(typeof(AnimationSamplerConverter))]
        public AnimationSampler Sampler { get; }

        public AnimationTarget Target { get; }
    }
}