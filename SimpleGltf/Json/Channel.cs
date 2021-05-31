using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    /*public class Channel
    {
        private readonly Animation _animation;

        internal Channel(Animation animation, AnimationSampler sampler, AnimationTarget target)
        {
            _animation = animation;
            _animation.Channels.Add(this);
            Sampler = sampler;
            Target = target;
        }

        [JsonIgnore] public AnimationSampler Sampler { get; }

        [JsonPropertyName("sampler")] public int SamplerReference => _animation.Samplers.IndexOf(Sampler);

        public AnimationTarget Target { get; }
    }*/
}