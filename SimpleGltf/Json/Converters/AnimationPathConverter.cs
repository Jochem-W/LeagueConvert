using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Converters
{
    public class AnimationPathConverter : JsonConverter<AnimationPath>
    {
        public override AnimationPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AnimationPath value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLower());
        }
    }
}