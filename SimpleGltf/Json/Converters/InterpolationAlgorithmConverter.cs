using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Converters
{
    internal class InterpolationAlgorithmConverter : JsonConverter<InterpolationAlgorithm>
    {
        public override InterpolationAlgorithm Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, InterpolationAlgorithm value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpper());
        }
    }
}