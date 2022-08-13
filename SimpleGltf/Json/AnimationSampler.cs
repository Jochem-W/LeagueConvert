using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class AnimationSampler : IIndexable
{
    internal AnimationSampler(Animation animation, FloatAccessor input, FloatAccessor output)
    {
        if (input.Type != AccessorType.Scalar)
        {
            throw new ArgumentException("Input has to be a scalar accessor with floats!", nameof(input));
        }

        Index = animation.SamplerList.Count;
        animation.SamplerList.Add(this);
        Input = input;
        Output = output;
    }

    [JsonConverter(typeof(IndexableConverter<Accessor>))]
    public Accessor Input { get; }

    [JsonConverter(typeof(IndexableConverter<Accessor>))]
    public Accessor Output { get; }

    [JsonIgnore] public int Index { get; }
}