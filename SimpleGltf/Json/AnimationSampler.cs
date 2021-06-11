using System;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationSampler
    {
        private const InterpolationAlgorithm DefaultInterpolationAlgorithm = InterpolationAlgorithm.Linear;
        internal readonly int Index;
        private InterpolationAlgorithm _interpolationAlgorithm;

        internal AnimationSampler(Animation animation, FloatAccessor input, IAccessor output)
        {
            if (input.Type != AccessorType.Scalar)
                throw new ArgumentException("Input has to be a scalar accessor with floats!", nameof(input));
            Index = animation.Samplers.Count;
            animation.Samplers.Add(this);
            Input = input;
            Output = output;
        }

        [JsonIgnore] public IAccessor Input { get; }

        [JsonPropertyName("input")] public int InputIndex => Input.Index;

        [JsonConverter(typeof(InterpolationAlgorithmConverter))]
        public InterpolationAlgorithm? Interpolation
        {
            get => _interpolationAlgorithm == DefaultInterpolationAlgorithm ? null : _interpolationAlgorithm;
            set => _interpolationAlgorithm = value ?? DefaultInterpolationAlgorithm;
        }

        [JsonIgnore] public IAccessor Output { get; }

        [JsonPropertyName("output")] public int OutputIndex => Output.Index;
    }
}