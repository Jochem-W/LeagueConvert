using System;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class AnimationSampler : IIndexable
    {
        internal AnimationSampler(Animation animation, FloatAccessor input, IAccessor output)
        {
            if (input.Type != AccessorType.Scalar)
                throw new ArgumentException("Input has to be a scalar accessor with floats!", nameof(input));
            Index = animation.Samplers.Count;
            animation.Samplers.Add(this);
            Input = input;
            Output = output;
        }

        [JsonConverter(typeof(IndexableConverter<IAccessor>))]
        public IAccessor Input { get; }

        [JsonConverter(typeof(IndexableConverter<IAccessor>))]
        public IAccessor Output { get; }

        [JsonIgnore] public int Index { get; }
    }
}