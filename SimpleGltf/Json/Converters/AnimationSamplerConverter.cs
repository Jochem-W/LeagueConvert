using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class AnimationSamplerConverter : JsonConverter<AnimationSampler>
    {
        public override AnimationSampler Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AnimationSampler value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Animation.Samplers.IndexOf(value));
        }
    }
}