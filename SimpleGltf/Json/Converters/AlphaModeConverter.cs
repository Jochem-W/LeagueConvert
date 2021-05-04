using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Converters
{
    public class AlphaModeConverter : JsonConverter<AlphaMode>
    {
        public override AlphaMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AlphaMode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpper());
        }
    }
}