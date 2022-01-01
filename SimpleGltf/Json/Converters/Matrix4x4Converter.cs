using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json.Converters
{
    internal class Matrix4x4Converter : JsonConverter<Matrix4x4>
    {
        public override Matrix4x4 Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializerOptions options)
        {
            value = value.Transpose();
            writer.WriteStartArray();
            writer.WriteNumberValue(value.M11);
            writer.WriteNumberValue(value.M12);
            writer.WriteNumberValue(value.M13);
            writer.WriteNumberValue(value.M14);
            writer.WriteNumberValue(value.M21);
            writer.WriteNumberValue(value.M22);
            writer.WriteNumberValue(value.M23);
            writer.WriteNumberValue(value.M24);
            writer.WriteNumberValue(value.M31);
            writer.WriteNumberValue(value.M32);
            writer.WriteNumberValue(value.M33);
            writer.WriteNumberValue(value.M34);
            writer.WriteNumberValue(value.M41);
            writer.WriteNumberValue(value.M42);
            writer.WriteNumberValue(value.M43);
            writer.WriteNumberValue(value.M44);
            writer.WriteEndArray();
        }
    }
}