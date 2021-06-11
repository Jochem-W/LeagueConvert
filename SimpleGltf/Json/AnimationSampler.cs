using System;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationSampler
    {
        private const InterpolationAlgorithm DefaultInterpolationAlgorithm = InterpolationAlgorithm.Linear;
        private InterpolationAlgorithm _interpolationAlgorithm;

        internal readonly int Index;
        internal readonly Animation Animation;

        internal AnimationSampler(Animation animation, FloatAccessor input, IAccessor output)
        {
            if (input.Type != AccessorType.Scalar)
                throw new ArgumentException("Input has to be a scalar accessor with floats!", nameof(input));
            Animation = animation;
            Index = Animation.Samplers.Count;
            Animation.Samplers.Add(this);
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