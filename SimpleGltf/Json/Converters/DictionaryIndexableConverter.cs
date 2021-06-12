using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    internal class DictionaryIndexableConverter<T> : JsonConverter<IDictionary<string, T>> where T : IIndexable
    {
        public override IDictionary<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var (name, v) in value)
                writer.WriteNumber(name, v.Index);
            writer.WriteEndObject();
        }
    }
}