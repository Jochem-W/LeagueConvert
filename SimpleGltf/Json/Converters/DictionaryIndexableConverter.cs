using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    internal class DictionaryIndexableConverter<T> : JsonConverter<IEnumerable<KeyValuePair<string, T>>>
        where T : IIndexable
    {
        public override IEnumerable<KeyValuePair<string, T>> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, T>> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var (name, v) in value)
                writer.WriteNumber(name, v.Index);
            writer.WriteEndObject();
        }
    }
}