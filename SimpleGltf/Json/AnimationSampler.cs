using System;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationSampler
    {
        private const InterpolationAlgorithm DefaultInterpolationAlgorithm = InterpolationAlgorithm.Linear;
        internal readonly Animation Animation;
        private InterpolationAlgorithm _interpolationAlgorithm;

        internal AnimationSampler(Animation animation, FloatAccessor input, IAccessor output)
        {
            if (input.Type != AccessorType.Scalar)
                throw new ArgumentException("Input has to be a scalar accessor with floats!", nameof(input));
            Animation = animation;
            Animation.Samplers.Add(this);
            Input = input;
            Output = output;
        }

        [JsonConverter(typeof(IAccessorConverter))]
        public IAccessor Input { get; }

        [JsonConverter(typeof(InterpolationAlgorithmConverter))]
        public InterpolationAlgorithm? Interpolation
        {
            get => _interpolationAlgorithm == DefaultInterpolationAlgorithm ? null : _interpolationAlgorithm;
            set => _interpolationAlgorithm = value ?? DefaultInterpolationAlgorithm;
        }

        [JsonConverter(typeof(IAccessorConverter))]
        public IAccessor Output { get; }
    }
}