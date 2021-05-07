namespace SimpleGltf.Json.Extensions
{
    public static class AnimationExtensions
    {
        public static Channel CreateChannel(this Animation animation, AnimationSampler sampler, AnimationTarget target)
        {
            return new(animation, sampler, target);
        }

        public static AnimationSampler CreateSampler(this Animation animation, Accessor input, Accessor output)
        {
            return new(animation, input, output);
        }
    }
}