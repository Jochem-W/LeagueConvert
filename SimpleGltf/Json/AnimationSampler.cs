using System;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    /*public class AnimationSampler<T> where T: struct, IComparable
    {
        private const InterpolationAlgorithm DefaultInterpolationAlgorithm = InterpolationAlgorithm.Linear;
        private InterpolationAlgorithm _interpolationAlgorithm;
        
        internal AnimationSampler(Animation animation, Accessor<float> input, Accessor<T> output)
        {
            if (input.Type != AccessorType.Scalar)
                throw new ArgumentException("Input has to be a scalar accessor with floats!", nameof(input));
            animation.Samplers.Add(this);
            Input = input;
            Output = output;
        }

        [JsonIgnore] public Accessor<float> Input { get; }

        [JsonPropertyName("input")] public int InputReference => Input.GltfAsset.Accessors.IndexOf(Input);

        [JsonConverter(typeof(InterpolationAlgorithmConverter))]
        public InterpolationAlgorithm? Interpolation
        {
            get => _interpolationAlgorithm == DefaultInterpolationAlgorithm ? null : _interpolationAlgorithm;
            set => _interpolationAlgorithm = value ?? DefaultInterpolationAlgorithm;
        }

        [JsonIgnore] public Accessor<T> Output { get; }

        [JsonPropertyName("output")] public int OutputReference => Output.GltfAsset.Accessors.IndexOf(Output);
    }*/
}