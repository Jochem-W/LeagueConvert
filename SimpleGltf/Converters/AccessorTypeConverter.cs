using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Converters
{
    public class AccessorTypeConverter : JsonConverter<AccessorType>
    {
        public override AccessorType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (Enum.TryParse<AccessorType>(reader.GetString(), out var result))
                return result;
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, AccessorType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpper());
        }
    }
}