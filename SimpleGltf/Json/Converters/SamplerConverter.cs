using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class SamplerConverter : JsonConverter<Sampler>
    {
        public override Sampler Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Sampler value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Samplers.IndexOf(value));
        }
    }
}