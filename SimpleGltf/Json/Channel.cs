using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Channel
    {
        internal Channel(Animation animation, AnimationSampler sampler, AnimationTarget target)
        {
            animation.Channels.Add(this);
            Sampler = sampler;
            Target = target;
        }

        [JsonIgnore] public AnimationSampler Sampler { get; }

        [JsonPropertyName("sampler")] public int SamplerIndex => Sampler.Index;

        public AnimationTarget Target { get; }
    }
}