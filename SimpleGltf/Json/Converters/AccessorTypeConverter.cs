using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Enums;

namespace SimpleGltf.Json.Converters
{
    public class AccessorTypeConverter : JsonConverter<AccessorType>
    {
        public override AccessorType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AccessorType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpper());
        }
    }
}