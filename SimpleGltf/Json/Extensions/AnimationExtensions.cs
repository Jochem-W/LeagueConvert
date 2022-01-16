namespace SimpleGltf.Json.Extensions;

public static class AnimationExtensions
{
    public static AnimationChannel CreateChannel(this Animation animation, AnimationSampler sampler,
        AnimationChannelTarget animationChannelTarget)
    {
        return new AnimationChannel(animation, sampler, animationChannelTarget);
    }

    public static AnimationSampler CreateSampler(this Animation animation, FloatAccessor input,
        FloatAccessor output)
    {
        return new AnimationSampler(animation, input, output);
    }
}