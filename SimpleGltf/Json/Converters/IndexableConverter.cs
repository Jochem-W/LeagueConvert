using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    internal class IndexableConverter<T> : JsonConverter<T> where T : IIndexable
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
                return;
            writer.WriteNumberValue(value.Index);
        }
    }
}