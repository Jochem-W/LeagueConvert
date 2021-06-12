namespace SimpleGltf.Json.Extensions
{
    public static class AnimationExtensions
    {
        public static AnimationChannel CreateChannel(this Animation animation, AnimationSampler sampler,
            AnimationTarget target)
        {
            return new(animation, sampler, target);
        }

        public static AnimationSampler CreateSampler(this Animation animation, FloatAccessor input, IAccessor output)
        {
            return new(animation, input, output);
        }
    }
}